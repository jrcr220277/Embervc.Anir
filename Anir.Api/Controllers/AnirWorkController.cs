using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Shared.Contracts.AnirWorks;
using Anir.Shared.Contracts.AnirWorks.Persons;
using Anir.Shared.Contracts.AnirWorks.Presentations;
using Anir.Shared.Contracts.Common;
using DocumentFormat.OpenXml.Office2016.Excel;
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

    public AnirWorkController(
        ApplicationDbContext db,
        ILogger<AnirWorkController> logger,
        IPdfService pdfService)
    {
        _db = db;
        _logger = logger;
        _pdfService = pdfService;
    }

    // ============================================================
    // MAPEOS PRIVADOS
    // ============================================================
    private static void MapDtoToEntity(AnirWorkDto dto, AnirWork entity)
    {
        entity.CompanyId = dto.CompanyId;
        entity.Date = dto.Date;
        entity.AnirNumber = dto.AnirNumber;
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.IsPaid = dto.IsPaid;
        entity.IsGeneralized = dto.IsGeneralized;
        entity.EconomicImpact = dto.EconomicImpact;
        entity.Recommendations = dto.Recommendations;
        entity.ResolutionNumber = dto.ResolutionNumber;
        entity.ImageId = dto.ImageId;
        entity.PdfId = dto.PdfId;
    }

    private static AnirWorkDto MapEntityToDto(AnirWork entity)
    {
        return new AnirWorkDto
        {
            Id = entity.Id,
            CompanyId = entity.CompanyId,
            CompanyName = entity.Company?.Name,
            Date = entity.Date,
            AnirNumber = entity.AnirNumber,
            Title = entity.Title,
            Description = entity.Description,
            IsPaid = entity.IsPaid,
            IsGeneralized = entity.IsGeneralized,
            EconomicImpact = entity.EconomicImpact,
            Recommendations = entity.Recommendations,
            ResolutionNumber = entity.ResolutionNumber,
            ImageId = entity.ImageId,
            PdfId = entity.PdfId,

            Persons = entity.AnirWorkPersons.Select(p => new AnirWorkPersonDto
            {
                Id = p.Id,
                PersonId = p.PersonId,
                PersonName = p.Person?.FullName,
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
        try
        {
            var query = _db.AnirWorks
                .Include(w => w.Company)
                .AsNoTracking();

            // Búsqueda
            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var s = queryDto.Search.Trim().ToLower();
                query = query.Where(w =>
                    w.Title.ToLower().Contains(s) ||
                    w.AnirNumber.ToLower().Contains(s) ||
                    (w.Description != null && w.Description.ToLower().Contains(s)) ||
                    (w.Company != null && w.Company.Name.ToLower().Contains(s))
                );
            }

            // Filtros
            if (queryDto.CompanyId.HasValue)
                query = query.Where(w => w.CompanyId == queryDto.CompanyId.Value);

            if (queryDto.IsPaid.HasValue)
                query = query.Where(w => w.IsPaid == queryDto.IsPaid.Value);

            if (queryDto.IsGeneralized.HasValue)
                query = query.Where(w => w.IsGeneralized == queryDto.IsGeneralized.Value);

            if (queryDto.FromDate.HasValue)
                query = query.Where(w => w.Date >= queryDto.FromDate.Value);

            if (queryDto.ToDate.HasValue)
                query = query.Where(w => w.Date <= queryDto.ToDate.Value);

            // Ordenamiento
            var orderedQuery = query.ApplySorting(queryDto);

            // Paginado + proyección
            var pagedResult = await orderedQuery
                .Select(w => MapEntityToDto(w))
                .ToPagedResultAsync(queryDto, ct);

            return Ok(ProcessResponse<PagedResponse<AnirWorkDto>>.Success(pagedResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error obteniendo {ENTITY.ToLower()}.");
            return StatusCode(500,
                ProcessResponse<PagedResponse<AnirWorkDto>>.Fail(
                    $"Ocurrió un error al obtener los {ENTITY.ToLower()}."
                ));
        }
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<AnirWorkDto>>> GetById(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var entity = await _db.AnirWorks
                .Include(w => w.Company)
                .Include(w => w.AnirWorkPersons).ThenInclude(p => p.Person)
                .Include(w => w.AnirWorkPresentations)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, ct);

            if (entity == null)
                return NotFound(ProcessResponse<AnirWorkDto>.Fail($"{ENTITY} no encontrado."));

            var dto = MapEntityToDto(entity);

            return Ok(ProcessResponse<AnirWorkDto>.Success(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo {Entity} {Id}", ENTITY, id);
            return StatusCode(500,
                ProcessResponse<AnirWorkDto>.Fail(
                    $"Ocurrió un error al obtener el {ENTITY.ToLower()}."
                ));
        }
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

        using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
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
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Error inesperado al crear {Entity}", ENTITY);
            return StatusCode(500,
                ProcessResponse<AnirWorkDto>.Fail(
                    $"Ocurrió un error al crear el {ENTITY.ToLower()}."
                ));
        }
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

        using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var entity = await _db.AnirWorks
                .Include(w => w.AnirWorkPersons)
                .Include(w => w.AnirWorkPresentations)
                .FirstOrDefaultAsync(w => w.Id == id, ct);

            if (entity == null)
                return NotFound(ProcessResponse<AnirWorkDto>.Fail($"{ENTITY} no encontrado."));

            // Mapear cambios
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
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Error inesperado al actualizar {Entity}", ENTITY);
            return StatusCode(500,
                ProcessResponse<AnirWorkDto>.Fail(
                    $"Ocurrió un error al actualizar el {ENTITY.ToLower()}."
                ));
        }
    }

    // ============================================================
    // DELETE
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProcessResponse<bool>>> Delete(int id, CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var entity = await _db.AnirWorks
                .Include(w => w.AnirWorkPersons)
                .Include(w => w.AnirWorkPresentations)
                .FirstOrDefaultAsync(w => w.Id == id, ct);

            if (entity == null)
                return NotFound(ProcessResponse<bool>.Fail($"{ENTITY} no encontrado."));

            _db.AnirWorkPersons.RemoveRange(entity.AnirWorkPersons);
            _db.AnirWorkPresentations.RemoveRange(entity.AnirWorkPresentations);
            _db.AnirWorks.Remove(entity);

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Ok(ProcessResponse<bool>.Success(true, $"{ENTITY} eliminado correctamente."));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Error inesperado al eliminar {Entity}", ENTITY);
            return StatusCode(500,
                ProcessResponse<bool>.Fail(
                    $"Ocurrió un error al eliminar el {ENTITY.ToLower()}."
                ));
        }
    }

    // ============================================================
    // BATCH DELETE
    // ============================================================
    [HttpPost("batch-delete")]
    public async Task<ActionResult<ProcessResponse<int>>> DeleteBatch(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
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
                _db.AnirWorkPersons.RemoveRange(w.AnirWorkPersons);
                _db.AnirWorkPresentations.RemoveRange(w.AnirWorkPresentations);
            }

            _db.AnirWorks.RemoveRange(items);

            var affected = await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Ok(ProcessResponse<int>.Success(affected, $"Se eliminaron {items.Count} {ENTITY.ToLower()}."));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, $"Error inesperado en eliminación masiva de {ENTITY.ToLower()}");
            return StatusCode(500,
                ProcessResponse<int>.Fail(
                    $"Ocurrió un error al eliminar los {ENTITY.ToLower()}."
                ));
        }
    }

    // ============================================================
    // EXPORT PDF LIST
    // ============================================================
    [HttpPost("export-pdf")]
    public async Task<IActionResult> ExportPdfList(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        IQueryable<AnirWork> query = _db.AnirWorks.Include(w => w.Company);

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
        IQueryable<AnirWork> query = _db.AnirWorks.Include(w => w.Company);

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
