using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using Anir.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private const string ENTITY = "Persona";


    private readonly ApplicationDbContext _db;
    private readonly ILogger<CompanyController> _logger;
    private readonly IPdfService _pdfService;
    private readonly ExcelService _excelService;

    public CompanyController(ApplicationDbContext db, ILogger<CompanyController> logger, IPdfService pdfService, ExcelService excelService)
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
    private static void MapDtoToEntity(CompanyDto dto, Company entity)
    {
        entity.ShortName = dto.ShortName;
        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.MunicipalityId = dto.MunicipalityId;
        entity.Active = dto.Active;
    }

    // Leer - listar - paginar 
    private static CompanyDto MapEntityToDto(Company c)
    {
        return new CompanyDto
        {
            Id = c.Id,
            ShortName = c.ShortName,
            Name = c.Name,
            Address = c.Address,
            MunicipalityId = c.MunicipalityId,
            MunicipalityName = c.Municipality?.Name,
            ProvinceName = c.Municipality?.Province?.Name,
            Active = c.Active
        };
    }

    // ============================================================
    // POST PAGED
    // ============================================================
    [HttpPost("getpaged")]
    public async Task<ActionResult<ProcessResponse<PagedResponse<CompanyDto>>>> GetPaged(
        [FromBody] CompanyQueryDto queryDto,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ProcessResponse<PagedResponse<CompanyDto>>.Fail("Datos inválidos."));

            // 🔥 ÚNICA LÍNEA QUE CAMBIAS CUANDO COPIAS ESTE CONTROLADOR
            var query = _db.Companies
                .Include(c => c.Municipality)
                .ThenInclude(m => m.Province)
                .AsNoTracking();

            // ------------------------------------------------------------
            // Búsqueda
            // ------------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var s = queryDto.Search.Trim().ToLower();

                query = query.Where(c =>
                    c.ShortName.ToLower().Contains(s) ||
                    c.Name.ToLower().Contains(s) ||
                    (c.Address != null && c.Address.ToLower().Contains(s)) ||
                    (c.Municipality != null && c.Municipality.Name.ToLower().Contains(s)) ||
                    (c.Municipality != null && c.Municipality.Province != null && c.Municipality.Province.Name.ToLower().Contains(s))
                );
            }

            // ------------------------------------------------------------
            // Filtros
            // ------------------------------------------------------------
            if (queryDto.ActiveFilter.HasValue)
                query = query.Where(c => c.Active == queryDto.ActiveFilter.Value);

            if (queryDto.MunicipalityId.HasValue)
                query = query.Where(c => c.MunicipalityId == queryDto.MunicipalityId.Value);

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

            return Ok(ProcessResponse<PagedResponse<CompanyDto>>.Success(pagedResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ocurrió un error al obtener la {ENTITY.ToLower()}.");
            return StatusCode(500, ProcessResponse<PagedResponse<CompanyDto>>.Fail($"Ocurrió un error al obtener la {ENTITY.ToLower()}."));
        }
    }



    // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> GetById(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            // Incluimos solo lo necesario para el DTO
            var entity = await _db.Companies
                .Include(c => c.Municipality)
                .ThenInclude(m => m.Province)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (entity == null)
                return NotFound(ProcessResponse<CompanyDto>.Fail($"{ENTITY} no encontrada."));

            var dto = MapEntityToDto(entity);

            return Ok(ProcessResponse<CompanyDto>.Success(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo {Entity} {Id}", ENTITY, id);
            return StatusCode(500,
                ProcessResponse<CompanyDto>.Fail($"Ocurrió un error al obtener la {ENTITY.ToLower()}."));
        }
    }

    // ============================================================
    // CREATE
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> Create(
        [FromBody] CompanyDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<CompanyDto>.Fail("Datos inválidos."));

        var entity = new Company();
        MapDtoToEntity(dto, entity);

        try
        {
            _db.Companies.Add(entity);
            await _db.SaveChangesAsync(ct);

            // Actualizamos el DTO con los datos generados
            dto.Id = entity.Id;
            dto.MunicipalityName = await _db.Municipalities
                .Where(m => m.Id == entity.MunicipalityId)
                .Select(m => m.Name)
                .FirstOrDefaultAsync(ct);

            return Ok(ProcessResponse<CompanyDto>.Success(dto, $"{ENTITY} creada correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear {Entity}", ENTITY);
            return StatusCode(500,
                ProcessResponse<CompanyDto>.Fail($"Ocurrió un error al crear la {ENTITY.ToLower()}."));
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> Update(int id, [FromBody] CompanyDto dto, CancellationToken ct = default)
    {
        // Validación de coherencia entre ruta y cuerpo
        if (id != dto.Id)
            return BadRequest(ProcessResponse<CompanyDto>.Fail(
                "El ID de la ruta no coincide con el del cuerpo."));

        // Buscar entidad existente
        var entity = await _db.Companies.FindAsync(new object?[] { id }, ct);

        if (entity == null)
            return NotFound(ProcessResponse<CompanyDto>.Fail($"{ENTITY} no encontrada."));

        // Mapear cambios del DTO a la entidad
        MapDtoToEntity(dto, entity);

        try
        {
            await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<CompanyDto>.Success(dto, $"{ENTITY} actualizada correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar {Entity}", ENTITY);

            return StatusCode(500,
                ProcessResponse<CompanyDto>.Fail(
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
            var entity = await _db.Companies.FindAsync(new object?[] { id }, ct);

            if (entity == null)
                return NotFound(ProcessResponse<bool>.Fail($"{ENTITY} no encontrada."));

            _db.Companies.Remove(entity);
            await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<bool>.Success(
                true,
                $"{ENTITY} eliminada correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar {Entity}", ENTITY);

            return StatusCode(500,
                ProcessResponse<bool>.Fail(
                    $"Ocurrió un error al eliminar la {ENTITY.ToLower()}."));
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
        try
        {
            IQueryable<Company> query = _db.Companies;
            List<Company> itemsToDelete;

            // ------------------------------------------------------------
            // MODO SELECT ALL (el usuario quiere eliminar TODO el filtro)
            // ------------------------------------------------------------
            if (request.SelectAll)
            {
                var filterDto = JsonSerializer.Deserialize<CompanyQueryDto>(
                    ((JsonElement)request.Filters).GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (filterDto == null)
                    return BadRequest(ProcessResponse<int>.Fail("Filtros inválidos."));

                // Búsqueda
                if (!string.IsNullOrWhiteSpace(filterDto.Search))
                {
                    var s = filterDto.Search.Trim().ToLower();
                    query = query.Where(c =>
                        c.ShortName.ToLower().Contains(s) ||
                        c.Name.ToLower().Contains(s) ||
                        (c.Address != null && c.Address.ToLower().Contains(s))
                    );
                }

                // Filtros
                if (filterDto.ActiveFilter.HasValue)
                    query = query.Where(c => c.Active == filterDto.ActiveFilter.Value);

                if (filterDto.MunicipalityId.HasValue)
                    query = query.Where(c => c.MunicipalityId == filterDto.MunicipalityId.Value);

                itemsToDelete = await query.ToListAsync(ct);
            }
            // ------------------------------------------------------------
            // MODO IDS ESPECÍFICOS
            // ------------------------------------------------------------
            else
            {
                if (request.Ids == null || request.Ids.Count == 0)
                    return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

                itemsToDelete = await query
                    .Where(c => request.Ids.Contains(c.Id))
                    .ToListAsync(ct);
            }

            // ------------------------------------------------------------
            // VALIDACIÓN
            // ------------------------------------------------------------
            if (!itemsToDelete.Any())
                return NotFound(ProcessResponse<int>.Fail($"No se encontraron {ENTITY.ToLower()}s para eliminar."));

            // ------------------------------------------------------------
            // ELIMINACIÓN
            // ------------------------------------------------------------
            _db.Companies.RemoveRange(itemsToDelete);
            var affectedRows = await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<int>.Success(
                affectedRows,
                $"Se eliminaron {itemsToDelete.Count} {ENTITY.ToLower()}s."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en eliminación masiva de {Entity}s", ENTITY);

            return StatusCode(500,
                ProcessResponse<int>.Fail($"Ocurrió un error al eliminar las {ENTITY.ToLower()}s."));
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
        IQueryable<Company> query = _db.Companies;

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        var items = await query
            .Select(c => MapEntityToDto(c))
            .ToListAsync(ct);

        var doc = new CompanyReportPdf(items);
        var pdfBytes = await _pdfService.GenerateAsync(doc, ct);

        return File(pdfBytes, "application/pdf");
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

        var items = await query
            .Select(c => MapEntityToDto(c))
            .ToListAsync(ct);

        var excelBytes = _excelService.GenerateCompaniesExcel(items);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "CompaniesReport.xlsx");
    }

}
