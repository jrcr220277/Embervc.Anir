using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using Anir.Shared.Contracts.Organisms;
using Anir.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private const string ENTITY = "Empresa";

    private readonly ApplicationDbContext _db;
    private readonly ILogger<CompanyController> _logger;
    private readonly IPdfService _pdfService;
    private readonly CompanyReportExcel _excelService;

    public CompanyController(ApplicationDbContext db, ILogger<CompanyController> logger, IPdfService pdfService, CompanyReportExcel excelService)
    {
        _db = db;
        _logger = logger;
        _pdfService = pdfService;
        _excelService = excelService;
    }

    // ============================================================
    // MÉTODOS PRIVADOS DE MAPEOS
    // ============================================================
    private static void MapDtoToEntity(CompanyDto dto, Company entity)
    {
        entity.Code = dto.Code;
        entity.ShortName = dto.ShortName;
        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.MunicipalityId = dto.MunicipalityId;
        entity.OrganismId = dto.OrganismId;
        entity.Active = dto.Active;
    }

    private static CompanyDto MapEntityToDto(Company entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        ShortName = entity.ShortName,
        Name = entity.Name,
        Address = entity.Address,
        Phone = entity.Phone,
        Email = entity.Email,
        MunicipalityId = entity.MunicipalityId,
        MunicipalityName = entity.Municipality?.Name,
        ProvinceName = entity.Municipality?.Province?.Name,
        OrganismId = entity.OrganismId,
        OrganismName = entity.Organism?.Name,
        Active = entity.Active
    };


    // ============================================================
    // GET ALL
    // ============================================================
    [HttpGet("all")]
    public async Task<ActionResult<List<CompanyDto>>> GetAll(CancellationToken ct = default)
    {
        var items = await _db.Companies
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new CompanyDto
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
    public async Task<ActionResult<ProcessResponse<PagedResponse<CompanyDto>>>> GetPaged(
        [FromBody] CompanyQueryDto queryDto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<PagedResponse<CompanyDto>>.Fail("Datos inválidos."));

        var query = _db.Companies
            .Include(c => c.Municipality)
            .ThenInclude(m => m.Province)
            .Include(c => c.Organism)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var s = queryDto.Search.Trim().ToLower();
            query = query.Where(entity =>
                entity.Code.ToLower().Contains(s) ||
                entity.ShortName.ToLower().Contains(s) ||
                entity.Name.ToLower().Contains(s) ||
                (entity.Address != null && entity.Address.ToLower().Contains(s)) ||
                entity.Organism.ShortName.ToLower().Contains(s)
            );
        }

        if (queryDto.ActiveFilter.HasValue)
            query = query.Where(x => x.Active == queryDto.ActiveFilter.Value);

        var orderedQuery = query.ApplySorting(queryDto);

        var pagedResult = await orderedQuery
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Code = c.Code,
                ShortName = c.ShortName,
                Name = c.Name,
                Address = c.Address,
                MunicipalityId = c.MunicipalityId,
                MunicipalityName = c.Municipality!.Name,
                ProvinceName = c.Municipality!.Province!.Name,
                OrganismId = c.OrganismId,
                OrganismName = c.Organism.ShortName,
                Active = c.Active
            })
            .ToPagedResultAsync(queryDto, ct);

        return Ok(ProcessResponse<PagedResponse<CompanyDto>>.Success(pagedResult));
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> GetById(int id, CancellationToken ct = default)
    {
        var entity = await _db.Companies
            .Include(c => c.Municipality)
            .ThenInclude(m => m.Province)
            .Include(c => c.Organism)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (entity == null)
            return NotFound(ProcessResponse<CompanyDto>.Fail($"{ENTITY} no encontrada."));

        var dto = MapEntityToDto(entity);
        return Ok(ProcessResponse<CompanyDto>.Success(dto));
    }

    // ============================================================
    // CREATE
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> Create([FromBody] CompanyDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<CompanyDto>.Fail("Datos inválidos."));

        var entity = new Company();
        MapDtoToEntity(dto, entity);

        _db.Companies.Add(entity);
        await _db.SaveChangesAsync(ct);

        dto.Id = entity.Id;
        dto.MunicipalityName = await _db.Municipalities
            .Where(m => m.Id == entity.MunicipalityId)
            .Select(m => m.Name)
            .FirstOrDefaultAsync(ct);

        dto.OrganismName = await _db.Organisms
            .Where(o => o.Id == entity.OrganismId)
            .Select(o => o.Name)
            .FirstOrDefaultAsync(ct);

        return Ok(ProcessResponse<CompanyDto>.Success(dto, $"{ENTITY} creada correctamente."));
    }

    // ============================================================
    // UPDATE
    // ============================================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> Update(int id, [FromBody] CompanyDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest(ProcessResponse<CompanyDto>.Fail("El ID de la ruta no coincide con el del cuerpo."));

        var entity = await _db.Companies.FindAsync(new object?[] { id }, ct);
        if (entity == null)
            return NotFound(ProcessResponse<CompanyDto>.Fail($"{ENTITY} no encontrada."));

        MapDtoToEntity(dto, entity);
        await _db.SaveChangesAsync(ct);

        return Ok(ProcessResponse<CompanyDto>.Success(dto, $"{ENTITY} actualizada correctamente."));
    }

    // ============================================================
    // DELETE
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProcessResponse<bool>>> Delete(int id, CancellationToken ct = default)
    {
        var entity = await _db.Companies.FindAsync(new object?[] { id }, ct);
        if (entity == null)
            return NotFound(ProcessResponse<bool>.Fail($"{ENTITY} no encontrada."));

        _db.Companies.Remove(entity);
        await _db.SaveChangesAsync(ct);

        return Ok(ProcessResponse<bool>.Success(true, $"{ENTITY} eliminada correctamente."));
    }


    // ============================================================
    // DELETE BATCH
    // ============================================================
    // ============================================================
    // DELETE BATCH (versión simple y profesional)
    // ============================================================
    [HttpPost("batch-delete")]
    public async Task<ActionResult<ProcessResponse<int>>> DeleteBatch(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        List<Company> itemsToDelete;

        // Selección global o por IDs
        if (request.SelectAll)
        {
            itemsToDelete = await _db.Companies.ToListAsync(ct);
        }
        else
        {
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

            itemsToDelete = await _db.Companies
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
        _db.Companies.RemoveRange(itemsToDelete);
        var affectedRows = await _db.SaveChangesAsync(ct);

        return Ok(ProcessResponse<int>.Success(
            affectedRows,
            $"Se eliminaron {affectedRows} {ENTITY.ToLower()}."
        ));
    }


    // ============================================================
    // EXPORT EXCEL
    // ============================================================
    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcel([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        IQueryable<Company> query = _db.Companies;

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        var items = await query.Select(c => MapEntityToDto(c)).ToListAsync(ct);

        var excelBytes = _excelService.GenerateCompaniesExcel(items);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "CompaniesReport.xlsx");
    }
}
