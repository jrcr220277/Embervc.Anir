using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
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
    private readonly IFileStorage _storage;

    public AnirWorkController(
        ApplicationDbContext db,
        ILogger<AnirWorkController> logger,
        IPdfService pdfService,
         IFileStorage storage)
    {
        _db = db;
        _logger = logger;
        _pdfService = pdfService;
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

            // ⭐ ARCHIVOS (IGUAL QUE PERSONCONTROLLER)
            ImageId = entity.ImageId,
            ImageUrl = string.IsNullOrWhiteSpace(entity.ImageId)
                ? null
                : $"{Request.Scheme}://{Request.Host}/images/{entity.ImageId}",

            PdfId = entity.PdfId,
            PdfUrl = string.IsNullOrWhiteSpace(entity.PdfId)
                ? null
                : $"{Request.Scheme}://{Request.Host}/docs/{entity.PdfId}",

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

        // ============================================================
        // SEARCH
        // ============================================================
        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var s = queryDto.Search.Trim().ToLower();

            query = query.Where(w =>
                w.Title.ToLower().Contains(s) ||
                w.AnirNumber.ToLower().Contains(s) ||
                w.Ueb.Name.ToLower().Contains(s) ||
                w.Ueb.Company.Name.ToLower().Contains(s)
            );
        }

        // ============================================================
        // FILTERS
        // ============================================================
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

        // ============================================================
        // SORT
        // ============================================================
        queryDto.Sort = queryDto.Sort switch
        {
            "CompanyName" => "Ueb.Company.Name",
            "UebName" => "Ueb.Name",
            _ => queryDto.Sort
        };

        var orderedQuery = query.ApplySorting(queryDto);

        // ============================================================
        // PROJECTION
        // ============================================================
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

        // ============================================================
        // PAGING
        // ============================================================
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
                .Include(w => w.Ueb)
                .ThenInclude(u => u.Company)
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

        // Personas
        foreach (var p in dto.Persons)
        {
            _db.AnirWorkPersons.Add(new AnirWorkPerson
            {
                AnirWorkId = entity.Id,
                PersonId = p.PersonId,
                ParticipationPercentage = p.ParticipationPercentage
            });
        }

        // Presentaciones
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
            return BadRequest(ProcessResponse<AnirWorkDto>.Fail(
                "El ID de la ruta no coincide con el del cuerpo."));

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var entity = await _db.AnirWorks
            .Include(w => w.AnirWorkPersons)
            .Include(w => w.AnirWorkPresentations)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

        if (entity == null)
            return NotFound(ProcessResponse<AnirWorkDto>.Fail($"{ENTITY} no encontrado."));

        // Mapear cambios principales
        MapDtoToEntity(dto, entity);

        // Reemplazar personas
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

        // Reemplazar presentaciones
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
    // DELETE
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

        // ⭐ BORRAR ARCHIVOS ANTES DE BORRAR EL REGISTRO
        if (!string.IsNullOrEmpty(entity.ImageId))
            await _storage.DeleteAsync(entity.ImageId, "images");

        if (!string.IsNullOrEmpty(entity.PdfId))
            await _storage.DeleteAsync(entity.PdfId, "docs");


        _db.AnirWorkPersons.RemoveRange(entity.AnirWorkPersons);
        _db.AnirWorkPresentations.RemoveRange(entity.AnirWorkPresentations);
        _db.AnirWorks.Remove(entity);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

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

        foreach (var w in items)
        {
            // ⭐ BORRAR ARCHIVOS
            if (!string.IsNullOrEmpty(w.ImageId))
                await _storage.DeleteAsync(w.ImageId, "images");

            if (!string.IsNullOrEmpty(w.PdfId))
                await _storage.DeleteAsync(w.PdfId, "docs");

            // ⭐ BORRAR RELACIONES
            _db.AnirWorkPersons.RemoveRange(w.AnirWorkPersons);
            _db.AnirWorkPresentations.RemoveRange(w.AnirWorkPresentations);
        }

        _db.AnirWorks.RemoveRange(items);

        var affected = await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(ProcessResponse<int>.Success(
            affected,
            $"Se eliminaron {items.Count} {ENTITY.ToLower()}."
        ));
    }



    // ============================================================
    // EXPORT PDF LIST
    // ============================================================
    [HttpPost("export-pdf")]
    public async Task<IActionResult> ExportPdfList(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        IQueryable<AnirWork> query = _db.AnirWorks.Include(w => w.Ueb).ThenInclude(u => u.Company);


        if (request.Ids is { Count: > 0 })
            query = query.Where(w => request.Ids.Contains(w.Id));

        var items = await query
            .Select(w => MapEntityToDto(w))
            .ToListAsync(ct);

        var doc = new AnirWorkListPdf(items);
        var pdfBytes = await _pdfService.GenerateAsync(doc, ct);

        return File(pdfBytes, "application/pdf");
    }

    // ============================================================
    // EXPORT PDF DETAIL
    // ============================================================
    [HttpGet("export-pdf/{id:int}")]
    public async Task<IActionResult> ExportPdfDetail(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        IQueryable<AnirWork> query = _db.AnirWorks.Include(w => w.Ueb).ThenInclude(u => u.Company);

        if (request.Ids is { Count: > 0 })
            query = query.Where(w => request.Ids.Contains(w.Id));

        var items = await query
            .Select(w => MapEntityToDto(w))
            .ToListAsync(ct);

        var doc = new AnirWorkListPdf(items);
        var pdfBytes = await _pdfService.GenerateAsync(doc, ct);

        return File(pdfBytes, "application/pdf");
    }
}
