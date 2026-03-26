using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ApplicationDbContext db, ILogger<CompanyController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private Dictionary<string, string[]> GetValidationErrors()
    {
        return ModelState
            .Where(x => x.Value?.Errors.Any() == true)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
    }

    // ============================================================
    // POST PAGED (ÚNICO MÉTODO PARA PAGINAR)
    // ============================================================
    [HttpPost("getpaged")]
    public async Task<ActionResult<ProcessResponse<PagedResult<CompanyDto>>>> GetPaged(
    [FromBody] CompanyQueryDto queryDto,
    CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ProcessResponse<PagedResult<CompanyDto>>.Fail(
                    "Datos inválidos.", GetValidationErrors()));

            var companiesQuery = _db.Companies
                .Include(company => company.Municipality)
                .AsNoTracking();

            // Búsqueda portable (case-insensitive)
            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var searchTerm = queryDto.Search.Trim().ToLower();

                companiesQuery = companiesQuery.Where(company =>
                    company.ShortName.ToLower().Contains(searchTerm) ||
                    company.Name.ToLower().Contains(searchTerm) ||
                    (company.Address != null && company.Address.ToLower().Contains(searchTerm)) ||
                    (company.Municipality != null && company.Municipality.Name.ToLower().Contains(searchTerm))
                );
            }

            // Filtro por estado
            if (queryDto.ActiveFilter.HasValue)
                companiesQuery = companiesQuery.Where(company => company.Active == queryDto.ActiveFilter.Value);

            // Filtro por municipio
            if (queryDto.MunicipalityId.HasValue)
                companiesQuery = companiesQuery.Where(company => company.MunicipalityId == queryDto.MunicipalityId.Value);

            // Ordenamiento dinámico
            companiesQuery = companiesQuery.ApplySorting(queryDto);

            // Paginado + proyección
            var pagedResult = await companiesQuery
                .Select(company => new CompanyDto
                {
                    Id = company.Id,
                    ShortName = company.ShortName,
                    Name = company.Name,
                    Address = company.Address,
                    MunicipalityId = company.MunicipalityId,
                    MunicipalityName = company.Municipality!.Name,
                    Active = company.Active
                })
                .ToPagedResultAsync(queryDto, ct);

            return Ok(ProcessResponse<PagedResult<CompanyDto>>.Success(pagedResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo listado de empresas");
            return StatusCode(500, ProcessResponse<PagedResult<CompanyDto>>.Fail(
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
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> GetById(
        int id,
        CancellationToken ct = default)
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
    public async Task<ActionResult<ProcessResponse<CompanyDto>>> Create(
        [FromBody] CompanyDto dto,
        CancellationToken ct = default)
    {
        try
        {
            // Validación del modelo recibido
            if (!ModelState.IsValid)
                return BadRequest(ProcessResponse<CompanyDto>.Fail(
                    "Datos inválidos.", GetValidationErrors()));

            // ------------------------------------------------------------
            // Validación de duplicados
            // ------------------------------------------------------------
            var exists = await _db.Companies.AnyAsync(company =>
                company.Name == dto.Name &&
                company.MunicipalityId == dto.MunicipalityId,
                ct);

            if (exists)
            {
                _logger.LogWarning("Intento de crear empresa duplicada {Name}", dto.Name);
                return Conflict(ProcessResponse<CompanyDto>.Fail(
                    "Ya existe una empresa con ese nombre en ese municipio."));
            }

            // ------------------------------------------------------------
            // Mapeo DTO → Entidad
            // ------------------------------------------------------------
            var entity = new Company
            {
                ShortName = dto.ShortName,
                Name = dto.Name,
                Address = dto.Address,
                MunicipalityId = dto.MunicipalityId,
                Active = dto.Active
            };

            // ------------------------------------------------------------
            // Persistencia
            // ------------------------------------------------------------
            _db.Companies.Add(entity);
            await _db.SaveChangesAsync(ct);

            // ------------------------------------------------------------
            // Completar DTO con datos generados por la BD
            // ------------------------------------------------------------
            dto.Id = entity.Id;

            dto.MunicipalityName = await _db.Municipalities
                .Where(municipality => municipality.Id == entity.MunicipalityId)
                .Select(municipality => municipality.Name)
                .FirstOrDefaultAsync(ct);

            return Ok(ProcessResponse<CompanyDto>.Success(
                dto,
                "Empresa creada correctamente."));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear empresa");
            return StatusCode(500, ProcessResponse<CompanyDto>.Fail(
                "Error al guardar en la base de datos."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear empresa");
            return StatusCode(500, ProcessResponse<CompanyDto>.Fail(
                "Ocurrió un error al crear la empresa."));
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
        try
        {
            // Validación de consistencia entre ruta y cuerpo
            if (id != dto.Id)
                return BadRequest(ProcessResponse<CompanyDto>.Fail(
                    "El ID de la ruta no coincide con el del cuerpo."));

            // Validación del modelo recibido
            if (!ModelState.IsValid)
                return BadRequest(ProcessResponse<CompanyDto>.Fail(
                    "Datos inválidos.", GetValidationErrors()));

            // ------------------------------------------------------------
            // Obtener la entidad a actualizar
            // ------------------------------------------------------------
            var entity = await _db.Companies.FindAsync(new object?[] { id }, ct);

            if (entity == null)
            {
                _logger.LogWarning("Empresa no encontrada {Id}", id);
                return NotFound(ProcessResponse<CompanyDto>.Fail("Empresa no encontrada."));
            }

            // ------------------------------------------------------------
            // Validación de duplicados
            // ------------------------------------------------------------
            var exists = await _db.Companies.AnyAsync(company =>
                company.Id != id &&
                company.Name == dto.Name &&
                company.MunicipalityId == dto.MunicipalityId,
                ct);

            if (exists)
            {
                return Conflict(ProcessResponse<CompanyDto>.Fail(
                    "Ya existe otra empresa con ese nombre en ese municipio."));
            }

            // ------------------------------------------------------------
            // Mapeo DTO → Entidad (actualización)
            // ------------------------------------------------------------
            entity.ShortName = dto.ShortName;
            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.MunicipalityId = dto.MunicipalityId;
            entity.Active = dto.Active;

            // ------------------------------------------------------------
            // Persistencia
            // ------------------------------------------------------------
            await _db.SaveChangesAsync(ct);

            // Respuesta estándar de éxito
            return Ok(ProcessResponse<CompanyDto>.Success(
                dto,
                "Empresa actualizada correctamente."));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Conflictos de concurrencia (otro usuario modificó la entidad)
            _logger.LogError(ex, "Conflicto de concurrencia al actualizar empresa {Id}", id);
            return Conflict(ProcessResponse<CompanyDto>.Fail(
                "La empresa fue modificada por otro usuario."));
        }
        catch (DbUpdateException ex)
        {
            // Errores relacionados con la base de datos
            _logger.LogError(ex, "Error de base de datos al actualizar empresa {Id}", id);
            return StatusCode(500, ProcessResponse<CompanyDto>.Fail(
                "Error al actualizar la base de datos."));
        }
        catch (Exception ex)
        {
            // Errores inesperados
            _logger.LogError(ex, "Error inesperado al actualizar empresa {Id}", id);
            return StatusCode(500, ProcessResponse<CompanyDto>.Fail(
                "Ocurrió un error al actualizar la empresa."));
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

            IQueryable<Company> companiesQuery = _db.Companies;
            List<Company> companiesToDelete;

            // ------------------------------------------------------------
            // Modo: Select All (el usuario seleccionó "todos" con filtros)
            // ------------------------------------------------------------
            if (request.SelectAll)
            {
                // Validación del objeto Filters
                if (request.Filters is not JsonElement jsonFilters ||
                    jsonFilters.ValueKind != JsonValueKind.Object)
                {
                    return BadRequest(ProcessResponse<int>.Fail("Filtros inválidos."));
                }

                // Deserializar filtros a CompanyQueryDto
                var filterDto = JsonSerializer.Deserialize<CompanyQueryDto>(
                    jsonFilters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (filterDto == null)
                    return BadRequest(ProcessResponse<int>.Fail("Filtros inválidos."));

                // ------------------------------------------------------------
                // Aplicar filtros (versión portable y case-insensitive)
                // ------------------------------------------------------------
                if (!string.IsNullOrWhiteSpace(filterDto.Search))
                {
                    var searchTerm = filterDto.Search.Trim().ToLower();

                    companiesQuery = companiesQuery.Where(company =>
                        company.ShortName.ToLower().Contains(searchTerm) ||
                        company.Name.ToLower().Contains(searchTerm) ||
                        (company.Address != null && company.Address.ToLower().Contains(searchTerm))
                    );
                }

                if (filterDto.ActiveFilter.HasValue)
                    companiesQuery = companiesQuery.Where(company =>
                        company.Active == filterDto.ActiveFilter.Value);

                if (filterDto.MunicipalityId.HasValue)
                    companiesQuery = companiesQuery.Where(company =>
                        company.MunicipalityId == filterDto.MunicipalityId.Value);

                companiesToDelete = await companiesQuery.ToListAsync(ct);
            }
            else
            {
                // ------------------------------------------------------------
                // Modo: selección manual de IDs
                // ------------------------------------------------------------
                if (request.Ids == null || request.Ids.Count == 0)
                    return BadRequest(ProcessResponse<int>.Fail("No se recibieron Ids para eliminar."));

                companiesToDelete = await companiesQuery
                    .Where(company => request.Ids.Contains(company.Id))
                    .ToListAsync(ct);
            }

            // ------------------------------------------------------------
            // Validación final
            // ------------------------------------------------------------
            if (!companiesToDelete.Any())
                return NotFound(ProcessResponse<int>.Fail("No se encontraron empresas para eliminar."));

            // ------------------------------------------------------------
            // Eliminación masiva
            // ------------------------------------------------------------
            _db.Companies.RemoveRange(companiesToDelete);
            var affectedRows = await _db.SaveChangesAsync(ct);

            return Ok(ProcessResponse<int>.Success(
                affectedRows,
                $"Se eliminaron {companiesToDelete.Count} empresas."));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos en eliminación masiva de empresas");
            return Conflict(ProcessResponse<int>.Fail(
                "No se pudieron eliminar las empresas. Verifique relaciones."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en eliminación masiva de empresas");
            return StatusCode(500, ProcessResponse<int>.Fail(
                "Ocurrió un error al eliminar las empresas."));
        }
    }

}
