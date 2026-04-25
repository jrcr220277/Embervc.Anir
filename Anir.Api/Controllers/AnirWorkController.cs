using Anir.Application.Common.Interfaces;
using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Shared.Contracts.AnirWorks;
using Anir.Shared.Contracts.AnirWorks.Persons;
using Anir.Shared.Contracts.AnirWorks.Presentations;
using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnirWorkController : ControllerBase
{
    private const string ENTITY = "Trabajo ANIR";

    private readonly ApplicationDbContext _db;
    private readonly ILogger<AnirWorkController> _logger;
    private readonly IReportDataProvider _reportDataProvider;
    private readonly AnirWorkReportExce _excelService;
    private readonly IFileStorageService _fileStorage;

    public AnirWorkController(
        ApplicationDbContext db,
        ILogger<AnirWorkController> logger,
        IReportDataProvider reportDataProvider,
        AnirWorkReportExce excelService,
        IFileStorageService fileStorage)
    {
        _db = db;
        _logger = logger;
        _reportDataProvider = reportDataProvider; ;
        _excelService = excelService;
        _fileStorage = fileStorage;
    }

    // ============================================================
    // MAPEOS PRIVADOS
    // ============================================================

    private FileResponse? MapFileResponse(StoredFile? file)
    {
        if (file is null) return null;
        return new FileResponse
        {
            Id = file.Id,
            Url = $"{Request.Scheme}://{Request.Host}/api/files/{file.Id}",
            Name = file.OriginalName,
            Size = file.SizeBytes,
            Type = file.MimeType
        };
    }

    private static void MapDtoToEntity(AnirWorkDto dto, AnirWork entity)
    {
        entity.UebId = dto.UebId;
        entity.Date = dto.Date;
        entity.AnirNumber = dto.AnirNumber;
        entity.Title = dto.Title;
        entity.Description = dto.Description;

        entity.HasSocialEffect = dto.HasSocialEffect;
        entity.HasEconomicEffect = dto.HasEconomicEffect;
        entity.Category = dto.Category;
        entity.Generalization = dto.Generalization;
        entity.IsExperimental = dto.IsExperimental;
        entity.ExperimentalStartDate = dto.ExperimentalStartDate;
        entity.ExperimentalEndDate = dto.ExperimentalEndDate;

        entity.EconomicImpact = dto.EconomicImpact;
        entity.Recommendations = dto.Recommendations;
        entity.State = dto.State;
        entity.ResolutionNumber = dto.ResolutionNumber;

        // Solo guardamos el ID (int?) que viene dentro del FileResponse
        entity.ImageFileId = dto.ImageFile?.Id;
        entity.PdfFileId = dto.PdfFile?.Id;
    }

    private AnirWorkDto MapEntityToDto(AnirWork entity)
    {
        return new AnirWorkDto
        {
            Id = entity.Id,

            // Organización
            UebId = entity.UebId,
            UebName = entity.Ueb.Name,
            CompanyId = entity.Ueb.CompanyId,
            CompanyName = entity.Ueb.Company.Name,

            // Datos base
            Date = entity.Date,
            AnirNumber = entity.AnirNumber,
            Title = entity.Title,
            Description = entity.Description,

            // Efectos
            HasSocialEffect = entity.HasSocialEffect,
            HasEconomicEffect = entity.HasEconomicEffect,
            Category = entity.Category,
            Generalization = entity.Generalization,
            IsExperimental = entity.IsExperimental,
            ExperimentalStartDate = entity.ExperimentalStartDate,
            ExperimentalEndDate = entity.ExperimentalEndDate,

            // Economía
            EconomicImpact = entity.EconomicImpact,
            Recommendations = entity.Recommendations,
            State = entity.State,
            ResolutionNumber = entity.ResolutionNumber,

            // Archivos
            ImageFile = MapFileResponse(entity.ImageFile),
            PdfFile = MapFileResponse(entity.PdfFile),

            // Relaciones
            Persons = entity.AnirWorkPersons.Select(p => new AnirWorkPersonDto
            {
                Id = p.Id,
                PersonId = p.PersonId,
                PersonName = p.Person.FullName,
                ParticipationPercentage = p.ParticipationPercentage
            }).ToList(),

            Presentations = entity.AnirWorkPresentations.Select(pr => new AnirWorkPresentationDto
            {
                Id = pr.Id,
                PresentationDate = pr.PresentationDate,
                Notes = pr.Notes
            }).ToList()
        };
    }

    // ============================================================
    // GET PAGED
    // ============================================================
    [HttpPost("getpaged")]
    public async Task<ActionResult<ProcessResponse<PagedResponse<AnirWorkDto>>>> GetPaged(
        [FromBody] AnirWorkQueryDto queryDto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<PagedResponse<AnirWorkDto>>.Fail("Datos inválidos."));

        var query = _db.AnirWorks
                 .Include(w => w.Ueb).ThenInclude(u => u.Company)
                 .Where(w => w.Ueb != null && w.Ueb.Company != null) // ← ESTO EVITA EL 500
                 .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var s = queryDto.Search.Trim().ToLower();
            query = query.Where(w =>
                w.Title.ToLower().Contains(s) ||
                w.AnirNumber.ToLower().Contains(s) ||
                w.Ueb.Name.ToLower().Contains(s) ||
                w.Ueb.Company.Name.ToLower().Contains(s));
        }

        if (queryDto.CompanyId.HasValue)
            query = query.Where(w => w.Ueb.CompanyId == queryDto.CompanyId.Value);
        if (queryDto.UebId.HasValue)
            query = query.Where(w => w.UebId == queryDto.UebId.Value);
        if (queryDto.HasSocialEffect.HasValue)
            query = query.Where(w => w.HasSocialEffect == queryDto.HasSocialEffect.Value);
        if (queryDto.HasEconomicEffect.HasValue)
            query = query.Where(w => w.HasEconomicEffect == queryDto.HasEconomicEffect.Value);
        if (queryDto.FromDate.HasValue)
            query = query.Where(w => w.Date >= queryDto.FromDate.Value);
        if (queryDto.ToDate.HasValue)
            query = query.Where(w => w.Date <= queryDto.ToDate.Value);

        queryDto.Sort = queryDto.Sort switch
        {
            "CompanyName" => "Ueb.Company.Name",
            "UebName" => "Ueb.Name",
            _ => queryDto.Sort
        };
        var orderedQuery = query.ApplySorting(queryDto);

        // En grilla no cargamos archivos pesados
        var projectedQuery = orderedQuery.Select(w => new AnirWorkDto
        {
            Id = w.Id,
            UebId = w.UebId,
            UebName = w.Ueb.Name,
            CompanyId = w.Ueb.CompanyId,
            CompanyName = w.Ueb.Company.Name,
            Date = w.Date,
            AnirNumber = w.AnirNumber,
            Title = w.Title,
            HasSocialEffect = w.HasSocialEffect,
            HasEconomicEffect = w.HasEconomicEffect
        });

        var paged = await projectedQuery.ToPagedResultAsync(queryDto, ct);
        return Ok(ProcessResponse<PagedResponse<AnirWorkDto>>.Success(paged));
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<AnirWorkDto>>> GetById(
        int id,
        CancellationToken ct = default)
    {
        var entity = await _db.AnirWorks
            .Include(w => w.Ueb).ThenInclude(u => u.Company)
            .Include(w => w.ImageFile)      // ← NUEVO
            .Include(w => w.PdfFile)        // ← NUEVO
            .Include(w => w.AnirWorkPersons).ThenInclude(p => p.Person)
            .Include(w => w.AnirWorkPresentations)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, ct);

        if (entity == null)
            return NotFound(ProcessResponse<AnirWorkDto>.Fail($"{ENTITY} no encontrado."));

        var dto = MapEntityToDto(entity);
        return Ok(ProcessResponse<AnirWorkDto>.Success(dto));
    }

    // ============================================================
    // CREATE
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<ProcessResponse<AnirWorkDto>>> Create(
        [FromBody] AnirWorkDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<AnirWorkDto>.Fail("Datos inválidos."));

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var entity = new AnirWork();
        MapDtoToEntity(dto, entity);

        _db.AnirWorks.Add(entity);
        await _db.SaveChangesAsync(ct);

        foreach (var p in dto.Persons)
        {
            _db.AnirWorkPersons.Add(new AnirWorkPerson
            {
                AnirWorkId = entity.Id,
                PersonId = p.PersonId,
                ParticipationPercentage = p.ParticipationPercentage
            });
        }
        foreach (var pr in dto.Presentations)
        {
            _db.AnirWorkPresentations.Add(new AnirWorkPresentation
            {
                AnirWorkId = entity.Id,
                PresentationDate = pr.PresentationDate,
                Notes = pr.Notes
            });
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        dto.Id = entity.Id;
        return Ok(ProcessResponse<AnirWorkDto>.Success(dto, $"{ENTITY} creado correctamente."));
    }

    // ============================================================
    // UPDATE
    // ============================================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProcessResponse<AnirWorkDto>>> Update(
        int id,
        [FromBody] AnirWorkDto dto,
        CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest(ProcessResponse<AnirWorkDto>.Fail("El ID de la ruta no coincide."));

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var entity = await _db.AnirWorks
            .Include(w => w.AnirWorkPersons)
            .Include(w => w.AnirWorkPresentations)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

        if (entity == null)
            return NotFound(ProcessResponse<AnirWorkDto>.Fail($"{ENTITY} no encontrado."));

        // Guardar IDs viejos (ahora son int?)
        int? oldImageId = entity.ImageFileId;
        int? oldPdfId = entity.PdfFileId;

        MapDtoToEntity(dto, entity);
        await _db.SaveChangesAsync(ct);

        // Si cambiaron los archivos, borramos los viejos (el servicio ya sabe la carpeta)
        if (oldImageId.HasValue && oldImageId != entity.ImageFileId)
        {
            try
            {
                await _fileStorage.DeleteAsync(oldImageId.Value, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar la imagen antigua {Id}", oldImageId);
            }
        }

        if (oldPdfId.HasValue && oldPdfId != entity.PdfFileId)
        {
            try
            {
                await _fileStorage.DeleteAsync(oldPdfId.Value, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar la imagen antigua {Id}", oldImageId);
            }
        }

        _db.AnirWorkPersons.RemoveRange(entity.AnirWorkPersons);
        foreach (var p in dto.Persons)
        {
            _db.AnirWorkPersons.Add(new AnirWorkPerson
            {
                AnirWorkId = entity.Id,
                PersonId = p.PersonId,
                ParticipationPercentage = p.ParticipationPercentage
            });
        }

        _db.AnirWorkPresentations.RemoveRange(entity.AnirWorkPresentations);
        foreach (var pr in dto.Presentations)
        {
            _db.AnirWorkPresentations.Add(new AnirWorkPresentation
            {
                AnirWorkId = entity.Id,
                PresentationDate = pr.PresentationDate,
                Notes = pr.Notes
            });
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(ProcessResponse<AnirWorkDto>.Success(dto, $"{ENTITY} actualizado correctamente."));
    }

    // ============================================================
    // DELETE (individual)
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProcessResponse<bool>>> Delete(int id, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var entity = await _db.AnirWorks
            .Include(w => w.AnirWorkPersons)
            .Include(w => w.AnirWorkPresentations)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

        if (entity == null)
            return NotFound(ProcessResponse<bool>.Fail($"{ENTITY} no encontrado."));

        int? imageId = entity.ImageFileId;
        int? pdfId = entity.PdfFileId;

        _db.AnirWorkPersons.RemoveRange(entity.AnirWorkPersons);
        _db.AnirWorkPresentations.RemoveRange(entity.AnirWorkPresentations);
        _db.AnirWorks.Remove(entity);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        if (imageId.HasValue)
        {
            try { await _fileStorage.DeleteAsync(imageId.Value, ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo eliminar imagen {Id}", imageId); }
        }
        if (pdfId.HasValue)
        {
            try { await _fileStorage.DeleteAsync(pdfId.Value, ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo eliminar PDF {Id}", pdfId); }
        }

        return Ok(ProcessResponse<bool>.Success(true, $"{ENTITY} eliminado correctamente."));
    }

    // ============================================================
    // BATCH DELETE
    // ============================================================
    [HttpPost("batch-delete")]
    public async Task<ActionResult<ProcessResponse<int>>> DeleteBatch(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        List<AnirWork> items;
        if (request.SelectAll)
        {
            items = await _db.AnirWorks
                .Include(w => w.AnirWorkPersons)
                .Include(w => w.AnirWorkPresentations)
                .ToListAsync(ct);
        }
        else
        {
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids."));

            items = await _db.AnirWorks
                .Include(w => w.AnirWorkPersons)
                .Include(w => w.AnirWorkPresentations)
                .Where(w => request.Ids.Contains(w.Id))
                .ToListAsync(ct);
        }

        if (!items.Any())
            return NotFound(ProcessResponse<int>.Fail($"No se encontraron registros."));

        // Recolectamos solo los IDs de archivos
        var fileIdsToDelete = new List<int>();
        foreach (var w in items)
        {
            if (w.ImageFileId.HasValue) fileIdsToDelete.Add(w.ImageFileId.Value);
            if (w.PdfFileId.HasValue) fileIdsToDelete.Add(w.PdfFileId.Value);

            _db.AnirWorkPersons.RemoveRange(w.AnirWorkPersons);
            _db.AnirWorkPresentations.RemoveRange(w.AnirWorkPresentations);
        }

        _db.AnirWorks.RemoveRange(items);
        int affected = await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // Borramos archivos por ID
        foreach (var fileId in fileIdsToDelete)
        {
            try { await _fileStorage.DeleteAsync(fileId, ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo eliminar archivo {Id}", fileId); }
        }

        return Ok(ProcessResponse<int>.Success(affected, $"Se eliminaron {items.Count} registros."));
    }

    //// ============================================================
    //// EXPORTAR LISTADO A PDF (QuestPDF)
    //// ============================================================
    //[HttpPost("export-pdf")]
    //public async Task<IActionResult> ExportPdfList(
    //[FromBody] BulkSelectionRequest request,
    //CancellationToken ct = default)
    //{
    //    // Obtener configuración (Logo, colores, textos)
    //    var config = await _reportDataProvider.GetConfigAsync(ct);

    //    // Obtener datos del negocio
    //    var query = _db.AnirWorks
    //        .Include(c => c.Municipality)

    //        .AsNoTracking();

    //    if (!request.SelectAll && request.Ids?.Any() == true)
    //        query = query.Where(c => request.Ids.Contains(c.Id));

    //    var data = await query.ToListAsync(ct);
    //    var dtos = data.Select(MapEntityToDto).ToList();

    //    // Generar PDF directo (Sin IPdfService)
    //    var document = new AnirWorkReportPdf(dtos, config);
    //    var bytes = document.GeneratePdf();

    //    // Nombre de archivo profesional
    //    var fileName = $"{config.ShortName ?? "ANIR"}_Empresas_{DateTime.Now:yyyyMMdd}.pdf";

    //    return File(bytes, "application/pdf", fileName);
    //}


    // ============================================================
    // EXPORTAR LISTADO A EXCEL (ClosedXML)
    // ============================================================
    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcelList(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        var query = _db.AnirWorks
            .Include(w => w.Ueb).ThenInclude(u => u.Company)
            .AsNoTracking();

        if (!request.SelectAll && request.Ids?.Any() == true)
            query = query.Where(w => request.Ids.Contains(w.Id));

        var data = await query.ToListAsync(ct);

        // Mapeamos a DTOs 
        var dtos = data.Select(MapEntityToDto).ToList();

        // Llamamos a tu clase Excel (Tienes que crear este método en AnirWorkReportExce)
        var bytes = _excelService.GenerateAnirWorksExcel(dtos);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Listado_AnirWorks.xlsx");
    }
}