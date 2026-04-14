using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Persons;
using Anir.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Anir.Infrastructure.Storage;

namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
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

    private PersonDto MapEntityToDto(Person entity) => new()
    {
        Id = entity.Id,
        ImagenId = entity.ImagenId,
        ImagenUrl = string.IsNullOrWhiteSpace(entity.ImagenId) ? null : $"{Request.Scheme}://{Request.Host}/{entity.ImagenId}",
        Dni = entity.Dni,
        FullName = entity.FullName,
        CellPhone = entity.CellPhone,
        Email = entity.Email,
        Description = entity.Description,
        Active = entity.Active
    };

    // ============================================================
    // GET PAGED
    // ============================================================
    [HttpPost("getpaged")]
    public async Task<ActionResult<ProcessResponse<PagedResponse<PersonDto>>>> GetPaged(
     [FromBody] PersonQueryDto queryDto,
     CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<PagedResponse<PersonDto>>.Fail("Datos inválidos."));

        var query = _db.Persons.AsNoTracking();

        // ============================================================
        // SEARCH
        // ============================================================
        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var s = queryDto.Search.Trim();

            query = query.Where(x =>
                x.Dni.Contains(s) ||
                x.FullName.Contains(s) ||
                x.CellPhone.Contains(s) ||
                x.Email.Contains(s) ||
                x.ImagenId.Contains(s)
            );
        }

        // ============================================================
        // FILTER ACTIVE
        // ============================================================
        if (queryDto.ActiveFilter.HasValue)
            query = query.Where(x => x.Active == queryDto.ActiveFilter.Value);

        // ============================================================
        // SORT
        // ============================================================
        var orderedQuery = query.ApplySorting(queryDto);

        // ============================================================
        // PROJECTION
        // ============================================================
        var projectedQuery = orderedQuery.Select(x => new PersonDto
        {
            Id = x.Id,
            ImagenId = x.ImagenId,
            ImagenUrl = string.IsNullOrWhiteSpace(x.ImagenId)
                ? null
                : $"{Request.Scheme}://{Request.Host}/{x.ImagenId}",
            Dni = x.Dni,
            FullName = x.FullName,
            CellPhone = x.CellPhone,
            Email = x.Email,
            Description = x.Description,
            Active = x.Active
        });

        // ============================================================
        // PAGING
        // ============================================================
        var paged = await projectedQuery.ToPagedResultAsync(queryDto, ct);

        return Ok(ProcessResponse<PagedResponse<PersonDto>>.Success(paged));
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<PersonDto>>> GetById(int id, CancellationToken ct = default)
    {
        var entity = await _db.Persons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null)
            return NotFound(ProcessResponse<PersonDto>.Fail($"{ENTITY} no encontrada."));

        var dto = MapEntityToDto(entity);
        return Ok(ProcessResponse<PersonDto>.Success(dto));
    }

    // ============================================================
    // CREATE
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<ProcessResponse<PersonDto>>> Create([FromBody] PersonDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<PersonDto>.Fail("Datos inválidos."));

        var entity = new Person();
        MapDtoToEntity(dto, entity);

        _db.Persons.Add(entity);
        await _db.SaveChangesAsync(ct);

        dto.Id = entity.Id;
        return Ok(ProcessResponse<PersonDto>.Success(dto, $"{ENTITY} creada correctamente."));
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

        if (!string.IsNullOrWhiteSpace(entity.ImagenId) && entity.ImagenId != dto.ImagenId)
        {
            var parts = entity.ImagenId.Split('/');
            var folder = parts[0];
            var fileName = parts[1];
            await _storage.DeleteAsync(fileName, folder);
        }

        MapDtoToEntity(dto, entity);
        await _db.SaveChangesAsync(ct);

        return Ok(ProcessResponse<PersonDto>.Success(dto, $"{ENTITY} actualizada correctamente."));
    }

    // ============================================================
    // DELETE
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProcessResponse<bool>>> Delete(int id, CancellationToken ct = default)
    {
        var entity = await _db.Persons.FindAsync(new object?[] { id }, ct);
        if (entity == null)
            return NotFound(ProcessResponse<bool>.Fail($"{ENTITY} no encontrada."));

        _db.Persons.Remove(entity);
        await _db.SaveChangesAsync(ct);

        return Ok(ProcessResponse<bool>.Success(true, $"{ENTITY} eliminada correctamente."));
    }
    // ============================================================
    // DELETE BATCH (versión simple y profesional)
    // ============================================================
    [HttpPost("batch-delete")]
    public async Task<ActionResult<ProcessResponse<int>>> DeleteBatch(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        List<Person> itemsToDelete;

        // Selección global o por IDs
        if (request.SelectAll)
        {
            itemsToDelete = await _db.Persons.ToListAsync(ct);
        }
        else
        {
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

            itemsToDelete = await _db.Persons
                .Where(o => request.Ids.Contains(o.Id))
                .ToListAsync(ct);
        }

        if (!itemsToDelete.Any())
            return NotFound(ProcessResponse<int>.Fail($"No se encontraron {ENTITY.ToLower()} para eliminar."));

        // ============================================================
        // BORRADO REAL (sin validación previa)
        // Si hay dependencias, la BD lanzará FK violation
        // y el middleware devolverá un mensaje claro.
        // ============================================================
        _db.Persons.RemoveRange(itemsToDelete);
        var affectedRows = await _db.SaveChangesAsync(ct);

        return Ok(ProcessResponse<int>.Success(
            affectedRows,
            $"Se eliminaron {affectedRows} {ENTITY.ToLower()}."
        ));
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

        var items = await query.Select(c => MapEntityToDto(c)).ToListAsync(ct);

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

        var items = await query.Select(c => MapEntityToDto(c)).ToListAsync(ct);

        var excelBytes = _excelService.GeneratePersonsExcel(items);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "PersonsReport.xlsx");
    }
}
