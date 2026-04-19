using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Infrastructure.Storage;
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
    private readonly IPdfService _pdfService;
    private readonly AnirWorkReportExce _excelService;
    private readonly IFileStorage _storage;

    public AnirWorkController(
        ApplicationDbContext db,
        ILogger<AnirWorkController> logger,
        IPdfService pdfService,
        AnirWorkReportExce excelService,
        IFileStorage storage)
    {
        _db = db;
        _logger = logger;
        _pdfService = pdfService;
        _excelService = excelService;
        _storage = storage;
    }

    // ============================================================
    // MAPEOS PRIVADOS
    // ============================================================
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

        // Guardamos los IDs tal cual vienen del frontend (ya subidos)
        entity.ImageId = dto.ImageId;
        entity.PdfId = dto.PdfId;
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

            // Archivos (URL construida desde el controller)
            ImageId = entity.ImageId,
            ImageUrl = string.IsNullOrWhiteSpace(entity.ImageId)
                ? null
                : $"{Request.Scheme}://{Request.Host}/api/files/images/{entity.ImageId}",

            PdfId = entity.PdfId,
            PdfUrl = string.IsNullOrWhiteSpace(entity.PdfId)
                ? null
                : $"{Request.Scheme}://{Request.Host}/api/files/docs/{entity.PdfId}",

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
            .Include(w => w.Ueb)
                .ThenInclude(u => u.Company)
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

        // Agregar personas y presentaciones
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

        // Actualizar DTO de salida
        dto.Id = entity.Id;
        dto.ImageId = entity.ImageId;
        dto.PdfId = entity.PdfId;

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
            return BadRequest(ProcessResponse<AnirWorkDto>.Fail("El ID de la ruta no coincide con el del cuerpo."));

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var entity = await _db.AnirWorks
            .Include(w => w.AnirWorkPersons)
            .Include(w => w.AnirWorkPresentations)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

        if (entity == null)
            return NotFound(ProcessResponse<AnirWorkDto>.Fail($"{ENTITY} no encontrado."));

        // Guardar IDs viejos para posible eliminación posterior
        string? oldImageId = entity.ImageId;
        string? oldPdfId = entity.PdfId;

        // Aplicar cambios del DTO (los IDs vienen ya subidos por el frontend)
        MapDtoToEntity(dto, entity);

        // Guardar cambios en la entidad
        await _db.SaveChangesAsync(ct);

        // Si el frontend reemplazó el archivo (nuevo Id distinto), eliminar el antiguo
        if (!string.IsNullOrEmpty(oldImageId) && !string.IsNullOrEmpty(entity.ImageId) && oldImageId != entity.ImageId)
        {
            try
            {
                await _storage.DeleteAsync(oldImageId, "images");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar la imagen antigua {OldImageId}", oldImageId);
            }
        }

        if (!string.IsNullOrEmpty(oldPdfId) && !string.IsNullOrEmpty(entity.PdfId) && oldPdfId != entity.PdfId)
        {
            try
            {
                await _storage.DeleteAsync(oldPdfId, "docs");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar el PDF antiguo {OldPdfId}", oldPdfId);
            }
        }

        // Reemplazar personas y presentaciones
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

        // Actualizar DTO de salida
        dto.ImageId = entity.ImageId;
        dto.PdfId = entity.PdfId;

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

        // Guardar IDs de archivos para borrar después
        string? imageId = entity.ImageId;
        string? pdfId = entity.PdfId;

        // Eliminar relaciones y la entidad
        _db.AnirWorkPersons.RemoveRange(entity.AnirWorkPersons);
        _db.AnirWorkPresentations.RemoveRange(entity.AnirWorkPresentations);
        _db.AnirWorks.Remove(entity);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // Eliminar archivos físicos (fuera de la transacción)
        if (!string.IsNullOrEmpty(imageId))
        {
            try { await _storage.DeleteAsync(imageId, "images"); }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo eliminar la imagen {ImageId} del trabajo {Id}", imageId, id); }
        }
        if (!string.IsNullOrEmpty(pdfId))
        {
            try { await _storage.DeleteAsync(pdfId, "docs"); }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo eliminar el PDF {PdfId} del trabajo {Id}", pdfId, id); }
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
                return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

            items = await _db.AnirWorks
                .Include(w => w.AnirWorkPersons)
                .Include(w => w.AnirWorkPresentations)
                .Where(w => request.Ids.Contains(w.Id))
                .ToListAsync(ct);
        }

        if (!items.Any())
            return NotFound(ProcessResponse<int>.Fail($"No se encontraron {ENTITY.ToLower()} para eliminar."));

        var filesToDelete = new List<(string Folder, string Id)>();
        foreach (var w in items)
        {
            if (!string.IsNullOrEmpty(w.ImageId))
                filesToDelete.Add(("images", w.ImageId));
            if (!string.IsNullOrEmpty(w.PdfId))
                filesToDelete.Add(("docs", w.PdfId));

            _db.AnirWorkPersons.RemoveRange(w.AnirWorkPersons);
            _db.AnirWorkPresentations.RemoveRange(w.AnirWorkPresentations);
        }

        _db.AnirWorks.RemoveRange(items);
        int affected = await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        foreach (var (folder, id) in filesToDelete)
        {
            try { await _storage.DeleteAsync(id, folder); }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo eliminar el archivo {Id} en {Folder}", id, folder); }
        }

        return Ok(ProcessResponse<int>.Success(affected, $"Se eliminaron {items.Count} {ENTITY.ToLower()}."));
    }

    

    

    //[HttpPost("export-excel")]
    //public async Task<IActionResult> ExportExcelList(
    //    [FromBody] BulkSelectionRequest request,
    //    CancellationToken ct = default)
    //{
    //    IQueryable<AnirWork> query = _db.AnirWorks
    //        .Include(w => w.Ueb).ThenInclude(u => u.Company)
    //        .Include(w => w.AnirWorkPersons).ThenInclude(p => p.Person)
    //        .Include(w => w.AnirWorkPresentations);

    //    if (request.Ids != null && request.Ids.Count > 0)
    //        query = query.Where(w => request.Ids.Contains(w.Id));

    //    var items = await query.ToListAsync(ct);
    //    var dtos = items.Select(MapEntityToDto).ToList();

    //    var excelBytes = await _excelService.GenerateCompaniesExcel(dtos, ct);
    //    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AnirWorks_{DateTime.Now:yyyyMMdd}.xlsx");
    //}
}
