using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Organisms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganismController : ControllerBase
{
    // Para poner nombre amiglable de la entidad en los mensajes usarios
    private const string ENTITY = "Organismo";

    private readonly ApplicationDbContext _db;
    private readonly ILogger<OrganismController> _logger;
    private readonly IPdfService _pdfService;
    private readonly OrganismReportExcel _excelService;

    public OrganismController(ApplicationDbContext db, ILogger<OrganismController> logger, IPdfService pdfService, OrganismReportExcel excelService)
    {
        _db = db;
        _logger = logger;
        _pdfService = pdfService;
        _excelService = excelService;
    }

    // ============================================================
    // MÉTODOS PRIVADOS DE MAPEOS (LOS ÚNICOS QUE PEDISTE)
    // ============================================================

    // Crear - Actualizar
    private static void MapDtoToEntity(OrganismDto dto, Organism entity)
    {
        entity.Code = dto.Code;
        entity.ShortName = dto.ShortName;
        entity.Name = dto.Name;
    }

    // Leer - listar - paginar 
    private static OrganismDto MapEntityToDto(Organism entity)
    {
        return new OrganismDto
        {
            Id = entity.Id,
            Code = entity.Code,
            ShortName = entity.ShortName,
            Name = entity.Name,
        };
    }

    // ============================================================
    // POST PAGED
    // ============================================================
    [HttpPost("getpaged")]
    public async Task<ActionResult<ProcessResponse<PagedResponse<OrganismDto>>>> GetPaged([FromBody] OrganismQueryDto queryDto, CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ProcessResponse<PagedResponse<OrganismDto>>.Fail("Datos inválidos."));

            // 🔥 ÚNICA LÍNEA QUE CAMBIAS CUANDO COPIAS ESTE CONTROLADOR
            var query = _db.Organisms
                .AsNoTracking();

            // ------------------------------------------------------------
            // Búsqueda
            // ------------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var s = queryDto.Search.Trim().ToLower();

                query = query.Where(entity =>
                    entity.Code.ToLower().Contains(s) ||
                    entity.ShortName.ToLower().Contains(s) ||
                    entity.Name.ToLower().Contains(s)
                );
            }


            // ------------------------------------------------------------
            // Ordenamiento (🔥 usando tu extensión mejorada)
            // ------------------------------------------------------------
            var orderedQuery = query.ApplySorting(queryDto);

            // ------------------------------------------------------------
            // Paginado + proyección
            // ------------------------------------------------------------
            var pagedResult = await orderedQuery
                .Select(c => MapEntityToDto(c))
                .ToPagedResultAsync(queryDto, ct);

            return Ok(ProcessResponse<PagedResponse<OrganismDto>>.Success(pagedResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ocurrió un error al obtener la {ENTITY.ToLower()}.");
            return StatusCode(500, ProcessResponse<PagedResponse<OrganismDto>>.Fail($"Ocurrió un error al obtener la {ENTITY.ToLower()}."));
        }
    }



    // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<OrganismDto>>> GetById(int id, CancellationToken ct = default)
    {
        try
        {
            // Incluimos solo lo necesario para el DTO
            var entity = await _db.Organisms
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null)
                return NotFound(ProcessResponse<OrganismDto>.Fail($"{ENTITY} no encontrada."));

            var dto = MapEntityToDto(entity);

            return Ok(ProcessResponse<OrganismDto>.Success(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo {Entity} {Id}", ENTITY, id);
            return StatusCode(500,
                ProcessResponse<OrganismDto>.Fail($"Ocurrió un error al obtener la {ENTITY.ToLower()}."));
        }
    }

    // ============================================================
    // CREATE
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<ProcessResponse<OrganismDto>>> Create([FromBody] OrganismDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<OrganismDto>.Fail("Datos inválidos."));

        var entity = new Organism();
        MapDtoToEntity(dto, entity);

        try
        {
            _db.Organisms.Add(entity);
            await _db.SaveChangesAsync(ct);

            // Actualizamos el DTO con los datos generados
            dto.Id = entity.Id;

            return Ok(ProcessResponse<OrganismDto>.Success(dto, $"{ENTITY} creada correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear {Entity}", ENTITY);
            return StatusCode(500,
                ProcessResponse<OrganismDto>.Fail($"Ocurrió un error al crear la {ENTITY.ToLower()}."));
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProcessResponse<OrganismDto>>> Update(int id, [FromBody] OrganismDto dto, CancellationToken ct = default)
    {
        // Validación de coherencia entre ruta y cuerpo
        if (id != dto.Id)
            return BadRequest(ProcessResponse<OrganismDto>.Fail(
                "El ID de la ruta no coincide con el del cuerpo."));

        // Buscar entidad existente
        var entity = await _db.Organisms.FindAsync(new object?[] { id }, ct);

        if (entity == null)
            return NotFound(ProcessResponse<OrganismDto>.Fail($"{ENTITY} no encontrada."));

        // Mapear cambios del DTO a la entidad
        MapDtoToEntity(dto, entity);

        try
        {
            await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<OrganismDto>.Success(dto, $"{ENTITY} actualizada correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar {Entity}", ENTITY);

            return StatusCode(500,
                ProcessResponse<OrganismDto>.Fail(
                    $"Ocurrió un error al actualizar la {ENTITY.ToLower()}."));
        }
    }

    // ============================================================
    // DELETE
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProcessResponse<bool>>> Delete(int id, CancellationToken ct = default)
    {
        try
        {
            var entity = await _db.Organisms.FindAsync(new object?[] { id }, ct);

            if (entity == null)
                return NotFound(ProcessResponse<bool>.Fail($"{ENTITY} no encontrada."));

            _db.Organisms.Remove(entity);
            await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<bool>.Success(
                true,
                $"{ENTITY} eliminada correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar {Entity}", ENTITY);

            return StatusCode(500, ProcessResponse<bool>.Fail($"Ocurrió un error al eliminar la {ENTITY.ToLower()}."));
        }
    }


    [HttpPost("batch-delete")]
    public async Task<ActionResult<ProcessResponse<int>>> DeleteBatch(
     [FromBody] BulkSelectionRequest request,
     CancellationToken ct = default)
    {
        try
        {
            List<Organism> itemsToDelete;

            if (request.SelectAll)
            {
                // ⭐ TU LÓGICA: BORRAR TODO
                itemsToDelete = await _db.Organisms.ToListAsync(ct);
            }
            else
            {
                if (request.Ids == null || request.Ids.Count == 0)
                    return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

                itemsToDelete = await _db.Organisms
                    .Where(c => request.Ids.Contains(c.Id))
                    .ToListAsync(ct);
            }

            if (!itemsToDelete.Any())
                return NotFound(ProcessResponse<int>.Fail($"No se encontraro {ENTITY.ToLower()} para eliminar."));

            _db.Organisms.RemoveRange(itemsToDelete);
            var affectedRows = await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<int>.Success(affectedRows, $"Se eliminaron {itemsToDelete.Count} {ENTITY.ToLower()}."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error inesperado en eliminación masiva de {ENTITY.ToLower()}");
            return StatusCode(500,
                ProcessResponse<int>.Fail($"Ocurrió un error al eliminar las {ENTITY.ToLower()}."));
        }
    }



    // ============================================================
    // EXPORT PDF
    // ============================================================
    // ============================================================
    // EXPORT PDF
    // ============================================================
    [HttpPost("export-pdf")]
    public async Task<IActionResult> ExportPdf([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        IQueryable<Organism> query = _db.Organisms;

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        var items = await query
            .Select(c => MapEntityToDto(c))
            .ToListAsync(ct);

        var doc = new OrganismReportPdf(items);
        var pdfBytes = await _pdfService.GenerateAsync(doc, ct);

        return File(pdfBytes, "application/pdf");
    }


    // ============================================================
    // EXPORT EXCEL
    // ============================================================
    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcel([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        IQueryable<Organism> query = _db.Organisms;

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        var items = await query
            .Select(c => MapEntityToDto(c))
            .ToListAsync(ct);

        var excelBytes = _excelService.GenerateOrganismsExcel(items);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "OrganismsReport.xlsx");
    }

}
