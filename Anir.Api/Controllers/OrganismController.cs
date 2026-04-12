using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Organisms;
using Anir.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganismController : ControllerBase
{
    private const string ENTITY = "Organismo";

    private readonly ApplicationDbContext _db;
    private readonly ILogger<OrganismController> _logger;
    private readonly IPdfService _pdfService;
    private readonly OrganismReportExcel _excelService;

    public OrganismController(
        ApplicationDbContext db,
        ILogger<OrganismController> logger,
        IPdfService pdfService,
        OrganismReportExcel excelService)
    {
        _db = db;
        _logger = logger;
        _pdfService = pdfService;
        _excelService = excelService;
    }

    // ============================================================
    // MÉTODOS PRIVADOS DE MAPEOS
    // ============================================================
    private static void MapDtoToEntity(OrganismDto dto, Organism entity)
    {
        entity.Code = dto.Code;
        entity.ShortName = dto.ShortName;
        entity.Name = dto.Name;
    }

    private static OrganismDto MapEntityToDto(Organism entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        ShortName = entity.ShortName,
        Name = entity.Name,
    };



    // ============================================================
    // GET ALL
    // ============================================================
    [HttpGet("all")]
    public async Task<ActionResult<List<OrganismDto>>> GetAll(CancellationToken ct = default)
    {
        var items = await _db.Organisms
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new OrganismDto
            {
                Id = o.Id,
                Code = o.Code,
                ShortName = o.ShortName,
                Name = o.Name
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    // ============================================================
    // GET PAGED
    // ============================================================
    [HttpPost("getpaged")]
    public async Task<ActionResult<ProcessResponse<PagedResponse<OrganismDto>>>> GetPaged(
        [FromBody] OrganismQueryDto queryDto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<PagedResponse<OrganismDto>>.Fail("Datos inválidos."));

        var query = _db.Organisms.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var s = queryDto.Search.Trim().ToLower();
            query = query.Where(entity =>
                entity.Code.ToLower().Contains(s) ||
                entity.ShortName.ToLower().Contains(s) ||
                entity.Name.ToLower().Contains(s));
        }

        var orderedQuery = query.ApplySorting(queryDto);

        var pagedResult = await orderedQuery
            .Select(c => MapEntityToDto(c))
            .ToPagedResultAsync(queryDto, ct);

        return Ok(ProcessResponse<PagedResponse<OrganismDto>>.Success(pagedResult));
    }

     // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<OrganismDto>>> GetById(int id, CancellationToken ct = default)
    {
        var entity = await _db.Organisms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return NotFound(ProcessResponse<OrganismDto>.Fail($"{ENTITY} no encontrada."));

        var dto = MapEntityToDto(entity);
        return Ok(ProcessResponse<OrganismDto>.Success(dto));
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

        _db.Organisms.Add(entity);
        await _db.SaveChangesAsync(ct);

        dto.Id = entity.Id;
        return Ok(ProcessResponse<OrganismDto>.Success(dto, $"{ENTITY} creada correctamente."));
    }

    // ============================================================
    // UPDATE
    // ============================================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProcessResponse<OrganismDto>>> Update(int id, [FromBody] OrganismDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest(ProcessResponse<OrganismDto>.Fail("El ID de la ruta no coincide con el del cuerpo."));

        var entity = await _db.Organisms.FindAsync(new object?[] { id }, ct);
        if (entity == null)
            return NotFound(ProcessResponse<OrganismDto>.Fail($"{ENTITY} no encontrada."));

        MapDtoToEntity(dto, entity);
        await _db.SaveChangesAsync(ct);

        return Ok(ProcessResponse<OrganismDto>.Success(dto, $"{ENTITY} actualizada correctamente."));
    }

    // ============================================================
    // DELETE
    // ============================================================
    // ============================================================
    // DELETE
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProcessResponse<bool>>> Delete(int id, CancellationToken ct = default)
    {
        var entity = await _db.Organisms.FindAsync(new object?[] { id }, ct);
        if (entity == null)
            return NotFound(ProcessResponse<bool>.Fail($"{ENTITY} no encontrada."));

        // ============================================================
        // VALIDACIÓN PREVIA (FK: Organism → Companies)
        // ============================================================
        var hasCompanies = await _db.Companies
            .AnyAsync(c => c.OrganismId == id, ct);

        if (hasCompanies)
        {
            return BadRequest(new ProcessResponse<bool>
            {
                Result = ResponseStatus.Failed,
                ErrorMessage = "No se puede eliminar este organismo porque tiene dependencias asociadas.",
                ValidationErrors = new Dictionary<string, string[]>
            {
                { "OrganismosConRestricciones", new[] { id.ToString() } }
            }
            });
        }

        _db.Organisms.Remove(entity);
        await _db.SaveChangesAsync(ct);

        return Ok(ProcessResponse<bool>.Success(true, $"{ENTITY} eliminada correctamente."));
    }

    // ============================================================
    // DELETE BATCH (versión profesional con validación previa)
    // ============================================================
    [HttpPost("batch-delete")]
    public async Task<ActionResult<ProcessResponse<int>>> DeleteBatch(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        List<Organism> itemsToDelete;

        // Selección global o por IDs
        if (request.SelectAll)
        {
            itemsToDelete = await _db.Organisms.ToListAsync(ct);
        }
        else
        {
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

            itemsToDelete = await _db.Organisms
                .Where(o => request.Ids.Contains(o.Id))
                .ToListAsync(ct);
        }

        if (!itemsToDelete.Any())
            return NotFound(ProcessResponse<int>.Fail($"No se encontraron {ENTITY.ToLower()} para eliminar."));

        var idsToDelete = itemsToDelete.Select(o => o.Id).ToList();

        // ============================================================
        // VALIDACIÓN PREVIA (FK: Organism → Companies)
        // ============================================================
        var organismsWithCompanies = await _db.Companies
            .Where(c => idsToDelete.Contains(c.OrganismId))
            .Select(c => c.OrganismId)
            .Distinct()
            .ToListAsync(ct);

        if (organismsWithCompanies.Any())
        {
            return BadRequest(new ProcessResponse<int>
            {
                Result = ResponseStatus.Failed,
                ErrorMessage = "No se pueden eliminar algunos organismos debido a restricciones de datos.",
                ValidationErrors = new Dictionary<string, string[]>
            {
                { "OrganismosConRestricciones", organismsWithCompanies.Select(x => x.ToString()).ToArray() }
            }
            });
        }

        // ============================================================
        // BORRADO REAL (transaccional)
        // ============================================================
        _db.Organisms.RemoveRange(itemsToDelete);
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
        IQueryable<Organism> query = _db.Organisms;

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        var items = await query.Select(c => MapEntityToDto(c)).ToListAsync(ct);

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

        var items = await query.Select(c => MapEntityToDto(c)).ToListAsync(ct);

        var excelBytes = _excelService.GenerateOrganismsExcel(items);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "OrganismsReport.xlsx");
    }
}
