using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using Anir.Shared.Contracts.Persons;
using Anir.Shared.Helpers;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Anir.Infrastructure.Storage;


namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    // Para poner nombre amiglable de la entidad en los mensajes usarios
    private const string ENTITY = "Persona";

    private readonly ApplicationDbContext _db;
    private readonly ILogger<PersonController> _logger;
    private readonly IPdfService _pdfService;
    private readonly PersonReportExcel _excelService;
    private readonly IFileStorage _storage;


    public PersonController(ApplicationDbContext db, ILogger<PersonController> logger, IPdfService pdfService, PersonReportExcel excelService, IFileStorage storage)
    {
        _db = db;
        _logger = logger;
        _pdfService = pdfService;
        _excelService = excelService;
        _storage = storage;
    }

    // ============================================================
    // MÉTODOS PRIVADOS DE MAPEOS
    // ============================================================

    // Crear - Actualizar (DTO → Entidad)
    private static void MapDtoToEntity(PersonDto dto, Person entity)
    {
        entity.ImagenId = dto.ImagenId;
        entity.Dni = dto.Dni;
        entity.FullName = dto.FullName;
        entity.CellPhone = dto.CellPhone;
        entity.Email = dto.Email;
        entity.Description = dto.Description;
        entity.Active = dto.Active;
    }

    // Leer (Entidad → DTO) — SOLO en memoria
    private PersonDto MapEntityToDto(Person entity)
    {
        return new PersonDto
        {
            Id = entity.Id,
            ImagenId = entity.ImagenId,
            ImagenUrl = string.IsNullOrWhiteSpace(entity.ImagenId) ? null: $"{Request.Scheme}://{Request.Host}/{entity.ImagenId}",
            Dni = entity.Dni,
            FullName = entity.FullName,
            CellPhone = entity.CellPhone,
            Email = entity.Email,
            Description = entity.Description,
            Active = entity.Active
        };
    }

    // ============================================================
    // POST PAGED
    // ============================================================
    [HttpPost("getpaged")]
    public async Task<ActionResult<ProcessResponse<PagedResponse<PersonDto>>>> GetPaged(
        [FromBody] PersonQueryDto queryDto,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ProcessResponse<PagedResponse<PersonDto>>.Fail("Datos inválidos."));

            // ------------------------------------------------------------
            // Base Query
            // ------------------------------------------------------------
            var query = _db.Persons.AsNoTracking();

            // ------------------------------------------------------------
            // Búsqueda
            // ------------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var s = queryDto.Search.Trim();

                query = query.Where(x =>
                    x.Dni.Contains(s) ||
                    x.FullName.Contains(s));
            }

            // ------------------------------------------------------------
            // Filtros
            // ------------------------------------------------------------
            if (queryDto.ActiveFilter.HasValue)
                query = query.Where(x => x.Active == queryDto.ActiveFilter.Value);

            // ------------------------------------------------------------
            // Ordenamiento
            // ------------------------------------------------------------
            var orderedQuery = query.ApplySorting(queryDto);

            // ------------------------------------------------------------
            // Proyección segura (EF Core friendly)
            // ------------------------------------------------------------
            var projectedQuery = orderedQuery.Select(x => new PersonDto
            {
                Id = x.Id,
                ImagenId = x.ImagenId,
                Dni = x.Dni,
                FullName = x.FullName,
                CellPhone = x.CellPhone,
                Email = x.Email,
                Description = x.Description,
                Active = x.Active
            });

            // ------------------------------------------------------------
            // Paginado
            // ------------------------------------------------------------
            var paged = await projectedQuery.ToPagedResultAsync(queryDto, ct);

            // ------------------------------------------------------------
            // Agregar URL de imagen (solo en memoria)
            // ------------------------------------------------------------
            foreach (var item in paged.Items)
            {
                item.ImagenUrl = string.IsNullOrWhiteSpace(item.ImagenId)
                    ? null
                    : $"{Request.Scheme}://{Request.Host}/{item.ImagenId}";
            
            }

            return Ok(ProcessResponse<PagedResponse<PersonDto>>.Success(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo {Entity}", ENTITY);
            return StatusCode(500,
                ProcessResponse<PagedResponse<PersonDto>>.Fail($"Ocurrió un error al obtener la {ENTITY.ToLower()}."));
        }
    }



    // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<PersonDto>>> GetById(int id, CancellationToken ct = default)
    {
        try
        {
            // Incluimos solo lo necesario para el DTO
            var entity = await _db.Persons
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, ct);

            if (entity == null)
                return NotFound(ProcessResponse<PersonDto>.Fail($"{ENTITY} no encontrada."));

            var dto = MapEntityToDto(entity);

            return Ok(ProcessResponse<PersonDto>.Success(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo {Entity} {Id}", ENTITY, id);
            return StatusCode(500,
                ProcessResponse<PersonDto>.Fail($"Ocurrió un error al obtener la {ENTITY.ToLower()}."));
        }
    }

    // ============================================================
    // CREATE
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<ProcessResponse<PersonDto>>> Create([FromBody] PersonDto dto,  CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<PersonDto>.Fail("Datos inválidos."));

        var entity = new Person();
        MapDtoToEntity(dto, entity);

        try
        {
            _db.Persons.Add(entity);
            await _db.SaveChangesAsync(ct);

            // Actualizamos el DTO con los datos generados
            dto.Id = entity.Id;
           
            return Ok(ProcessResponse<PersonDto>.Success(dto, $"{ENTITY} creada correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear {Entity}", ENTITY);
            return StatusCode(500,
                ProcessResponse<PersonDto>.Fail($"Ocurrió un error al crear la {ENTITY.ToLower()}."));
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProcessResponse<PersonDto>>> Update(int id, [FromBody] PersonDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest(ProcessResponse<PersonDto>.Fail("El ID de la ruta no coincide con el del cuerpo."));

        var entity = await _db.Persons.FindAsync(id, ct);

        if (entity == null)
            return NotFound(ProcessResponse<PersonDto>.Fail($"{ENTITY} no encontrada."));

        try
        {
            // ⭐ 1. Si la imagen cambió, eliminar la anterior del HDD
            if (!string.IsNullOrWhiteSpace(entity.ImagenId) &&
                entity.ImagenId != dto.ImagenId)
            {
                var parts = entity.ImagenId.Split('/');
                var folder = parts[0];
                var fileName = parts[1];

                await _storage.DeleteAsync(fileName, folder);
            }

            // ⭐ 2. Mapear cambios del DTO a la entidad
            MapDtoToEntity(dto, entity);

            // ⭐ 3. Guardar cambios
            await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<PersonDto>.Success(dto, $"{ENTITY} actualizada correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar {Entity}", ENTITY);

            return StatusCode(500,
                ProcessResponse<PersonDto>.Fail($"Ocurrió un error al actualizar la {ENTITY.ToLower()}."));
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
            var entity = await _db.Persons.FindAsync(id, ct);

            if (entity == null)
                return NotFound(ProcessResponse<bool>.Fail($"{ENTITY} no encontrada."));

            _db.Persons.Remove(entity);
            await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<bool>.Success(true, $"{ENTITY} eliminada correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar {Entity}", ENTITY);

            return StatusCode(500, ProcessResponse<bool>.Fail($"Ocurrió un error al eliminar la {ENTITY.ToLower()}."));
        }
    }


    // ============================================================
    // BATCH DELETE
    // ============================================================
    [HttpPost("batch-delete")]
    public async Task<ActionResult<ProcessResponse<int>>> DeleteBatch([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        try
        {
            List<Person> itemsToDelete;

            if (request.SelectAll)
            {
                // ⭐ TU LÓGICA: BORRAR TODO
                itemsToDelete = await _db.Persons.ToListAsync(ct);
            }
            else
            {
                if (request.Ids == null || request.Ids.Count == 0)
                    return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

                itemsToDelete = await _db.Persons
                    .Where(c => request.Ids.Contains(c.Id))
                    .ToListAsync(ct);
            }

            if (!itemsToDelete.Any())
                return NotFound(ProcessResponse<int>.Fail($"No se encontraro {ENTITY.ToLower()} para eliminar."));

            _db.Persons.RemoveRange(itemsToDelete);
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
    [HttpPost("export-pdf")]
    public async Task<IActionResult> ExportPdf([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        IQueryable<Person> query = _db.Persons;

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        var items = await query
            .Select(c => MapEntityToDto(c))
            .ToListAsync(ct);

        var doc = new PersonReportPdf(items);
        var pdfBytes = await _pdfService.GenerateAsync(doc, ct);

        return File(pdfBytes, "application/pdf");
    }


    // ============================================================
    // EXPORT EXCEL
    // ============================================================
    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcel([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        IQueryable<Person> query = _db.Persons;

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        var items = await query
            .Select(c => MapEntityToDto(c))
            .ToListAsync(ct);

        var excelBytes = _excelService.GeneratePersonsExcel(items);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "CompaniesReport.xlsx");
    }

}
