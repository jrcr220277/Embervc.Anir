using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using Anir.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;

namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
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
    // POST PAGED (ÚNICO MÉTODO PARA PAGINAR)
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

            var companiesQuery = _db.Companies
                .Include(c => c.Municipality)
                .ThenInclude(m => m.Province) // necesario para ordenar por provincia
                .AsNoTracking();

            // ------------------------------------------------------------
            // Búsqueda portable (case-insensitive)
            // ------------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var searchTerm = queryDto.Search.Trim().ToLower();

                companiesQuery = companiesQuery.Where(c =>
                    c.ShortName.ToLower().Contains(searchTerm) ||
                    c.Name.ToLower().Contains(searchTerm) ||
                    (c.Address != null && c.Address.ToLower().Contains(searchTerm)) ||
                    (c.Municipality != null && c.Municipality.Name.ToLower().Contains(searchTerm)) ||
                    (c.Municipality != null && c.Municipality.Province != null && c.Municipality.Province.Name.ToLower().Contains(searchTerm))
                );
            }

            // ------------------------------------------------------------
            // Filtros
            // ------------------------------------------------------------
            if (queryDto.ActiveFilter.HasValue)
                companiesQuery = companiesQuery.Where(c => c.Active == queryDto.ActiveFilter.Value);

            if (queryDto.MunicipalityId.HasValue)
                companiesQuery = companiesQuery.Where(c => c.MunicipalityId == queryDto.MunicipalityId.Value);

            // ------------------------------------------------------------
            // Ordenamiento dinámico
            // ------------------------------------------------------------
            IOrderedQueryable<Company> orderedQuery;
           
            switch (queryDto.Sort)
            {
                case "MunicipalityName":
                    orderedQuery = queryDto.Desc
                        ? companiesQuery.OrderByDescending(c => c.Municipality.Name)
                        : companiesQuery.OrderBy(c => c.Municipality.Name);
                    break;

                case "ProvinceName":
                    orderedQuery = queryDto.Desc
                        ? companiesQuery.OrderByDescending(c => c.Municipality.Province.Name)
                        : companiesQuery.OrderBy(c => c.Municipality.Province.Name);
                    break;

                case "Name":
                    orderedQuery = queryDto.Desc
                        ? companiesQuery.OrderByDescending(c => c.Name)
                        : companiesQuery.OrderBy(c => c.Name);
                    break;

                case "ShortName":
                    orderedQuery = queryDto.Desc
                        ? companiesQuery.OrderByDescending(c => c.ShortName)
                        : companiesQuery.OrderBy(c => c.ShortName);
                    break;

                default:
                    orderedQuery = companiesQuery.OrderBy(c => c.ShortName); // fallback seguro
                    break;
            }

            // ------------------------------------------------------------
            // Paginado + proyección
            // ------------------------------------------------------------
            var pagedResult = await orderedQuery
                .Select(c => new CompanyDto
                {
                    Id = c.Id,
                    ShortName = c.ShortName,
                    Name = c.Name,
                    Address = c.Address,
                    ProvinceName = c.Municipality!.Province!.Name,
                    MunicipalityId = c.MunicipalityId,
                    MunicipalityName = c.Municipality!.Name,
                    Active = c.Active
                })
                .ToPagedResultAsync(queryDto, ct);

            return Ok(ProcessResponse<PagedResponse<CompanyDto>>.Success(pagedResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo listado de empresas");
            return StatusCode(500, ProcessResponse<PagedResponse<CompanyDto>>.Fail(
                "Ocurrió un error al obtener las empresas."));
        }
    }


    // ============================================================
    // GET BY ID
    // Obtiene una empresa por su identificador.
    // Este método sigue el patrón estándar que se replicará
    // para cualquier entidad del sistema (Person, Product, etc.)
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> GetById(int id, CancellationToken ct = default)
    {
        try
        {
            // Consulta de la entidad:
            // - Include para cargar Municipio (relación necesaria para el DTO)
            // - FirstOrDefaultAsync para obtener un único registro
            var company = await _db.Companies
                .Include(company => company.Municipality)
                .FirstOrDefaultAsync(company => company.Id == id, ct);

            // Si no existe, devolvemos 404 con un mensaje estándar
            if (company == null)
            {
                _logger.LogWarning("Empresa no encontrada {Id}", id);
                return NotFound(ProcessResponse<CompanyDto>.Fail("Empresa no encontrada."));
            }

            // Proyección manual a DTO:
            // Esto garantiza que nunca exponemos entidades directamente
            var dto = new CompanyDto
            {
                Id = company.Id,
                ShortName = company.ShortName,
                Name = company.Name,
                Address = company.Address,
                MunicipalityId = company.MunicipalityId,
                MunicipalityName = company.Municipality?.Name,
                Active = company.Active
            };

            // Respuesta estándar de éxito
            return Ok(ProcessResponse<CompanyDto>.Success(dto));
        }
        catch (Exception ex)
        {
            // Logueo del error para diagnóstico
            _logger.LogError(ex, "Error obteniendo empresa {Id}", id);

            // Respuesta estándar de error interno
            return StatusCode(500, ProcessResponse<CompanyDto>.Fail(
                "Ocurrió un error al obtener la empresa."));
        }
    }


    // ============================================================
    // CREATE
    // Crea una nueva empresa en el sistema.
    // Este patrón es replicable para cualquier entidad.
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> Create([FromBody] CompanyDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<CompanyDto>.Fail("Datos inválidos."));

        var entity = new Company
        {
            ShortName = dto.ShortName,
            Name = dto.Name,
            Address = dto.Address,
            MunicipalityId = dto.MunicipalityId,
            Active = dto.Active
        };

        try
        {
            _db.Companies.Add(entity);
            await _db.SaveChangesAsync(ct);

            dto.Id = entity.Id;
            dto.MunicipalityName = await _db.Municipalities
                .Where(m => m.Id == entity.MunicipalityId)
                .Select(m => m.Name)
                .FirstOrDefaultAsync(ct);

            return Ok(ProcessResponse<CompanyDto>.Success(dto, "Empresa creada correctamente."));
        }
        catch (DbUpdateException ex)
        {
            var inner = ex.InnerException ?? ex;

            if (DatabaseErrorHelper.IsUniqueViolation(inner))
                return Conflict(ProcessResponse<CompanyDto>.Fail("Ya existe una empresa con esos datos."));
                       
            _logger.LogError(ex, "Error inesperado de base de datos");
            return StatusCode(500, ProcessResponse<CompanyDto>.Fail("Error al guardar en la base de datos."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear empresa");
            return StatusCode(500, ProcessResponse<CompanyDto>.Fail("Ocurrió un error al crear la empresa."));
        }
    }

    // ============================================================
    // UPDATE
    // Actualiza una empresa existente en el sistema.
    // Este patrón es replicable para cualquier entidad.
    // ============================================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> Update(
    int id,
    [FromBody] CompanyDto dto,
    CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest(ProcessResponse<CompanyDto>.Fail("El ID de la ruta no coincide con el del cuerpo."));

        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<CompanyDto>.Fail("Datos inválidos."));

        var entity = await _db.Companies.FindAsync(new object?[] { id }, ct);

        if (entity == null)
        {
            _logger.LogWarning("Empresa no encontrada {Id}", id);
            return NotFound(ProcessResponse<CompanyDto>.Fail("Empresa no encontrada."));
        }

        // Validación opcional de duplicados (si quieres feedback rápido)
        var exists = await _db.Companies.AnyAsync(c =>
            c.Id != id &&
            c.Name == dto.Name &&
            c.MunicipalityId == dto.MunicipalityId, ct);

        if (exists)
            return Conflict(ProcessResponse<CompanyDto>.Fail("Ya existe otra empresa con ese nombre en ese municipio."));

        // Actualizar entidad
        entity.ShortName = dto.ShortName;
        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.MunicipalityId = dto.MunicipalityId;
        entity.Active = dto.Active;

        try
        {
            await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<CompanyDto>.Success(dto, "Empresa actualizada correctamente."));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Conflicto de concurrencia al actualizar empresa {Id}", id);
            return Conflict(ProcessResponse<CompanyDto>.Fail("La empresa fue modificada por otro usuario."));
        }
        catch (DbUpdateException ex)
        {
            var inner = ex.InnerException ?? ex;

            if (DatabaseErrorHelper.IsUniqueViolation(inner))
                return Conflict(ProcessResponse<CompanyDto>.Fail("Ya existe una empresa con esos datos."));

            _logger.LogError(ex, "Error de base de datos al actualizar empresa {Id}", id);
            return StatusCode(500, ProcessResponse<CompanyDto>.Fail("Error al guardar en la base de datos."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar empresa {Id}", id);
            return StatusCode(500, ProcessResponse<CompanyDto>.Fail("Ocurrió un error al actualizar la empresa."));
        }
    }



    // ============================================================
    // DELETE
    // Elimina una empresa por su identificador.
    // Este patrón es replicable para cualquier entidad.
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProcessResponse<bool>>> Delete(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            // ------------------------------------------------------------
            // Obtener la entidad a eliminar
            // ------------------------------------------------------------
            var entity = await _db.Companies.FindAsync(new object?[] { id }, ct);

            if (entity == null)
            {
                return NotFound(ProcessResponse<bool>.Fail(
                    "Empresa no encontrada."));
            }

            // ------------------------------------------------------------
            // Eliminación
            // ------------------------------------------------------------
            _db.Companies.Remove(entity);
            await _db.SaveChangesAsync(ct);

            // Respuesta estándar de éxito
            return Ok(ProcessResponse<bool>.Success(
                true,
                "Empresa eliminada correctamente."));
        }
        catch (DbUpdateException ex)
        {
            // Errores relacionados con restricciones de BD (FK, etc.)
            _logger.LogError(ex, "Error de base de datos al eliminar empresa {Id}", id);

            return Conflict(ProcessResponse<bool>.Fail(
                "No se pudo eliminar la empresa. Puede tener datos relacionados."));
        }
        catch (Exception ex)
        {
            // Errores inesperados
            _logger.LogError(ex, "Error inesperado al eliminar empresa {Id}", id);

            return StatusCode(500, ProcessResponse<bool>.Fail(
                "Ocurrió un error al eliminar la empresa."));
        }
    }


    // ============================================================
    // BATCH DELETE
    // Elimina múltiples empresas según selección directa o filtros.
    // Este patrón es replicable para cualquier entidad.
    // ============================================================
    [HttpPost("batch-delete")]
    public async Task<ActionResult<ProcessResponse<int>>> DeleteBatch(
     [FromBody] BulkSelectionRequest request,
     CancellationToken ct = default)
    {
        try
        {
            if (request == null)
                return BadRequest(ProcessResponse<int>.Fail("Request inválido."));

            IQueryable<Company> query = _db.Companies;
            List<Company> companiesToDelete;

            if (request.SelectAll)
            {
                var filterDto = JsonSerializer.Deserialize<CompanyQueryDto>(
                    ((JsonElement)request.Filters).GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (filterDto == null)
                    return BadRequest(ProcessResponse<int>.Fail("Filtros inválidos."));

                if (!string.IsNullOrWhiteSpace(filterDto.Search))
                {
                    var searchTerm = filterDto.Search.Trim().ToLower();
                    query = query.Where(c =>
                        c.ShortName.ToLower().Contains(searchTerm) ||
                        c.Name.ToLower().Contains(searchTerm) ||
                        (c.Address != null && c.Address.ToLower().Contains(searchTerm))
                    );
                }

                if (filterDto.ActiveFilter.HasValue)
                    query = query.Where(c => c.Active == filterDto.ActiveFilter.Value);

                if (filterDto.MunicipalityId.HasValue)
                    query = query.Where(c => c.MunicipalityId == filterDto.MunicipalityId.Value);

                companiesToDelete = await query.ToListAsync(ct);
            }
            else
            {
                if (request.Ids == null || request.Ids.Count == 0)
                    return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

                companiesToDelete = await query
                    .Where(c => request.Ids.Contains(c.Id))
                    .ToListAsync(ct);
            }

            if (!companiesToDelete.Any())
                return NotFound(ProcessResponse<int>.Fail("No se encontraron empresas para eliminar."));

            _db.Companies.RemoveRange(companiesToDelete);
            var affectedRows = await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<int>.Success(
                affectedRows,
                $"Se eliminaron {companiesToDelete.Count} empresas."));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos en eliminación masiva de empresas");
            return Conflict(ProcessResponse<int>.Fail("No se pudieron eliminar las empresas. Verifique relaciones."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en eliminación masiva de empresas");
            return StatusCode(500, ProcessResponse<int>.Fail("Ocurrió un error al eliminar las empresas."));
        }
    }

    [HttpPost("export-pdf")]
    public async Task<IActionResult> ExportPdf([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        IQueryable<Company> query = _db.Companies;

        if (request.Ids != null && request.Ids.Any())
            query = query.Where(c => request.Ids.Contains(c.Id));

        var companies = await query.Select(c => new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            MunicipalityName = c.Municipality!.Name,
            ProvinceName = c.Municipality!.Province!.Name,
            Active = c.Active
        }).ToListAsync(ct);

        var doc = new CompanyReportPdf(companies);
        var pdfBytes = await _pdfService.GenerateAsync(doc, ct);

        // Inline: el navegador abre el PDF directamente
        return File(pdfBytes, "application/pdf");

    }



    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcel([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        IQueryable<Company> query = _db.Companies;

        if (request.Ids != null && request.Ids.Any())
            query = query.Where(c => request.Ids.Contains(c.Id));

        var companies = await query.Select(c => new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            MunicipalityName = c.Municipality!.Name,
            ProvinceName = c.Municipality!.Province!.Name,
            Active = c.Active
        }).ToListAsync(ct);

        var excelBytes = _excelService.GenerateCompaniesExcel(companies);

        // Esto hace que el navegador lo descargue
        return File(excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "CompaniesReport.xlsx");
    }




}
