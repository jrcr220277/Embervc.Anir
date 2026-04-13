using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Uebs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UebController : ControllerBase
{
    private const string ENTITY = "Ueb";

    private readonly ApplicationDbContext _db;
    private readonly ILogger<UebController> _logger;
    private readonly IPdfService _pdfService;
    private readonly UebReportExcel _excelService;

    public UebController(ApplicationDbContext db, ILogger<UebController> logger, IPdfService pdfService, UebReportExcel excelService)
    {
        _db = db;
        _logger = logger;
        _pdfService = pdfService;
        _excelService = excelService;
    }

    // ============================================================
    // MÉTODOS PRIVADOS DE MAPEOS
    // ============================================================
    private static void MapDtoToEntity(UebDto dto, Ueb entity)
    {
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.MunicipalityId = dto.MunicipalityId;
        entity.CompanyId = dto.CompanyId;
        entity.Active = dto.Active;
    }

    private static UebDto MapEntityToDto(Ueb entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        Address = entity.Address,
        Phone = entity.Phone,
        Email = entity.Email,
        CompanyId = entity.CompanyId,
        CompanyName = entity.Company?.ShortName,
        MunicipalityId = entity.MunicipalityId,
        MunicipalityName = entity.Municipality?.Name,
        ProvinceName = entity.Municipality?.Province?.Name,
        Active = entity.Active
    };

    // ============================================================
    // GET PAGED
    // ============================================================
    [HttpPost("getpaged")]
    public async Task<ActionResult<ProcessResponse<PagedResponse<UebDto>>>> GetPaged(
        [FromBody] UebQueryDto queryDto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<PagedResponse<UebDto>>.Fail("Datos inválidos."));

        var query = _db.Uebs
            .Include(c => c.Municipality)
            .ThenInclude(m => m.Province)
            .Include(c => c.Company)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var s = queryDto.Search.Trim().ToLower();
            query = query.Where(entity =>
                entity.Company.ShortName.ToLower().Contains(s) ||
                entity.Code.ToLower().Contains(s) ||
                entity.Name.ToLower().Contains(s) ||
                entity.Phone.ToLower().Contains(s) ||
                entity.Email.ToLower().Contains(s) 
             
               
            );
        }

        if (queryDto.ActiveFilter.HasValue)
            query = query.Where(x => x.Active == queryDto.ActiveFilter.Value);

        var orderedQuery = query.ApplySorting(queryDto);

        var pagedResult = await orderedQuery
         .Select(c => new UebDto
         {
             Id = c.Id,
             Code = c.Code,
             Name = c.Name,
             Address = c.Address,
             Phone = c.Phone,
             Email = c.Email,
             MunicipalityId = c.MunicipalityId,
             MunicipalityName = c.Municipality!.Name,
             ProvinceName = c.Municipality!.Province!.Name,
             CompanyId = c.CompanyId,
             CompanyName = c.Company!.ShortName,
             Active = c.Active
         })
         .ToPagedResultAsync(queryDto, ct);

        return Ok(ProcessResponse<PagedResponse<UebDto>>.Success(pagedResult));
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<UebDto>>> GetById(int id, CancellationToken ct = default)
    {
        var entity = await _db.Uebs
            .Include(x => x.Municipality)
            .ThenInclude(x => x.Province)
            .Include(x => x.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return NotFound(ProcessResponse<UebDto>.Fail($"{ENTITY} no encontrada."));

        var dto = MapEntityToDto(entity);
        return Ok(ProcessResponse<UebDto>.Success(dto));
    }

    // ============================================================
    // CREATE
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<ProcessResponse<UebDto>>> Create(
        [FromBody] UebDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<UebDto>.Fail("Datos inválidos."));

        var entity = new Ueb();
        MapDtoToEntity(dto, entity);

        _db.Uebs.Add(entity);
        await _db.SaveChangesAsync(ct);

        // Recargar con relaciones para devolver DTO completo
        var fullEntity = await _db.Uebs
            .Include(x => x.Municipality).ThenInclude(x => x.Province)
            .Include(x => x.Company)
            .AsNoTracking()
            .FirstAsync(x => x.Id == entity.Id, ct);

        var resultDto = MapEntityToDto(fullEntity);

        return Ok(ProcessResponse<UebDto>.Success(resultDto, $"{ENTITY} creada correctamente."));
    }



    // ============================================================
    // UPDATE
    // ============================================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProcessResponse<UebDto>>> Update(
        int id,
        [FromBody] UebDto dto,
        CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest(ProcessResponse<UebDto>.Fail("El ID de la ruta no coincide con el del cuerpo."));

        var entity = await _db.Uebs.FindAsync(new object?[] { id }, ct);
        if (entity == null)
            return NotFound(ProcessResponse<UebDto>.Fail($"{ENTITY} no encontrada."));

        MapDtoToEntity(dto, entity);
        await _db.SaveChangesAsync(ct);

        // Recargar para devolver DTO completo
        var fullEntity = await _db.Uebs
            .Include(x => x.Municipality).ThenInclude(x => x.Province)
            .Include(x => x.Company)
            .AsNoTracking()
            .FirstAsync(x => x.Id == id, ct);

        var resultDto = MapEntityToDto(fullEntity);

        return Ok(ProcessResponse<UebDto>.Success(resultDto, $"{ENTITY} actualizada correctamente."));
    }


    // ============================================================
    // DELETE
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProcessResponse<bool>>> Delete(int id, CancellationToken ct = default)
    {
        var entity = await _db.Uebs.FindAsync(new object?[] { id }, ct);
        if (entity == null)
            return NotFound(ProcessResponse<bool>.Fail($"{ENTITY} no encontrada."));

        _db.Uebs.Remove(entity);
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
        List<Ueb> itemsToDelete;

        // Selección global o por IDs
        if (request.SelectAll)
        {
            itemsToDelete = await _db.Uebs.ToListAsync(ct);
        }
        else
        {
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

            itemsToDelete = await _db.Uebs
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
        _db.Uebs.RemoveRange(itemsToDelete);
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
        IQueryable<Ueb> query = _db.Uebs;

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        var items = await query.Select(c => MapEntityToDto(c)).ToListAsync(ct);

        var doc = new UebReportPdf(items);
        var pdfBytes = await _pdfService.GenerateAsync(doc, ct);

        return File(pdfBytes, "application/pdf");
    }

    // ============================================================
    // EXPORT EXCEL
    // ============================================================
    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcel([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        IQueryable<Ueb> query = _db.Uebs;

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        var items = await query.Select(c => MapEntityToDto(c)).ToListAsync(ct);

        var excelBytes = _excelService.GenerateUebsExcel(items);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "UebsReport.xlsx");
    }
}
