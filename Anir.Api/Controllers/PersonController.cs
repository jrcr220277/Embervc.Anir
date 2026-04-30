using Anir.Application.Common.Interfaces;
using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Extensions;
using Anir.Infrastructure.Reports;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Persons;
using Anir.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Anir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private const string ENTITY = "Persona";

    private readonly ApplicationDbContext _db;
    private readonly ILogger<PersonController> _logger;
    private readonly IReportDataProvider _reportDataProvider;
    private readonly PersonReportExcel _excelService;
    private readonly IFileStorageService _fileStorage;

    public PersonController(
        ApplicationDbContext db,
        ILogger<PersonController> logger,
        IReportDataProvider reportDataProvider,
        PersonReportExcel excelService,
        IFileStorageService fileStorage)
    {
        _db = db;
        _logger = logger;
        _reportDataProvider = reportDataProvider;
        _excelService = excelService;
        _fileStorage = fileStorage;
    }

    // ============================================================
    // MÉTODOS PRIVADOS DE MAPEOS
    // ============================================================
    private FileResponse? MapFileResponse(StoredFile? file)
    {
        if (file is null) return null;
        return new FileResponse
        {
            Id = file.Id,
            Url = $"{Request.Scheme}://{Request.Host}/api/files/{file.Id}",
            Name = file.OriginalName,
            Size = file.SizeBytes,
            Type = file.MimeType
        };
    }

    private static void MapDtoToEntity(PersonDto dto, Person entity)
    {
        entity.ImageFileId = dto.ImageFile?.Id;
        entity.Dni = dto.Dni;
        entity.FullName = dto.FullName;
        entity.CellPhone = dto.CellPhone;
        entity.Email = dto.Email;
        entity.Affiliation = dto.Affiliation;

        // ═══════════════════════════════════════════════════════════
        // NUEVOS CAMPOS ENUM - Mapeo
        // ═══════════════════════════════════════════════════════════
        entity.SchoolLevel = dto.SchoolLevel;
        entity.Sex = dto.Sex;
        entity.ExecutiveRole = dto.ExecutiveRole;
        entity.Militancy = dto.Militancy;
        // ═══════════════════════════════════════════════════════════

        entity.Description = dto.Description;
        entity.Active = dto.Active;
    }

    private PersonDto MapEntityToDto(Person entity) => new()
    {
        Id = entity.Id,
        ImageFile = MapFileResponse(entity.ImageFile),
        Dni = entity.Dni,
        FullName = entity.FullName,
        CellPhone = entity.CellPhone,
        Email = entity.Email,
        Affiliation = entity.Affiliation,

        // ═══════════════════════════════════════════════════════════
        // NUEVOS CAMPOS ENUM - Mapeo
        // ═══════════════════════════════════════════════════════════
        SchoolLevel = entity.SchoolLevel,
        Sex = entity.Sex,
        ExecutiveRole = entity.ExecutiveRole,
        Militancy = entity.Militancy,
        // ═══════════════════════════════════════════════════════════

        Description = entity.Description,
        Active = entity.Active
    };

    // ============================================================
    // SEARCH (Para Selects / Autocompleters)
    // ============================================================
    [HttpGet("search")]
    public async Task<ActionResult<List<PersonDto>>> Search(
        [FromQuery] string q,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<PersonDto>());

        var results = await _db.Persons
            .AsNoTracking()
            .Where(p => p.FullName.Contains(q) || p.Dni.Contains(q))
            .OrderBy(p => p.FullName)
            .Take(20)
            .Select(p => new PersonDto
            {
                Id = p.Id,
                FullName = p.FullName
                // El resto llega null. EF Core NO los consulta en la BD.
            })
            .ToListAsync(ct);

        return Ok(results);
    }


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

        // 1. CONSTRUIMOS LA URL AQUÍ (Fuera del LINQ, así EF Core no explota)
        string baseUrl = $"{Request.Scheme}://{Request.Host}";

        var query = _db.Persons.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var searchTerm = queryDto.Search.Trim();

            if (Enum.TryParse<Affiliation>(searchTerm, true, out var affiliation))
            {
                query = query.Where(x => x.Affiliation == affiliation);
            }
            // ═══════════════════════════════════════════════════════════
            // NUEVOS CAMPOS ENUM - Búsqueda
            // ═══════════════════════════════════════════════════════════
            else if (Enum.TryParse<SchoolLevel>(searchTerm, true, out var schoolLevel))
            {
                query = query.Where(x => x.SchoolLevel == schoolLevel);
            }
            else if (Enum.TryParse<Sex>(searchTerm, true, out var sex))
            {
                query = query.Where(x => x.Sex == sex);
            }
            else if (Enum.TryParse<ExecutiveRole>(searchTerm, true, out var executiveRole))
            {
                query = query.Where(x => x.ExecutiveRole == executiveRole);
            }
            else if (Enum.TryParse<Militancy>(searchTerm, true, out var militancy))
            {
                query = query.Where(x => x.Militancy == militancy);
            }
            // ═══════════════════════════════════════════════════════════
            else
            {
                query = query.Where(x =>
                    x.Dni.Contains(searchTerm) ||
                    x.FullName.Contains(searchTerm) ||
                    x.CellPhone.Contains(searchTerm) ||
                    x.Email.Contains(searchTerm));
            }
        }

        if (queryDto.ActiveFilter.HasValue)
            query = query.Where(x => x.Active == queryDto.ActiveFilter.Value);

        var orderedQuery = query.ApplySorting(queryDto);

        // 2. USA LA VARIABLE DENTRO DEL SELECT (EF Core la trata como texto fijo)
        var projectedQuery = orderedQuery.Select(x => new PersonDto
        {
            Id = x.Id,
            Dni = x.Dni,
            FullName = x.FullName,
            CellPhone = x.CellPhone,
            Email = x.Email,
            Affiliation = x.Affiliation,

            // ═══════════════════════════════════════════════════════════
            // NUEVOS CAMPOS ENUM - Proyección
            // ═══════════════════════════════════════════════════════════
            SchoolLevel = x.SchoolLevel,
            Sex = x.Sex,
            ExecutiveRole = x.ExecutiveRole,
            Militancy = x.Militancy,
            // ═══════════════════════════════════════════════════════════

            Description = x.Description,
            Active = x.Active,

            ImageFile = x.ImageFileId.HasValue
                ? new FileResponse
                {
                    Id = x.ImageFileId.Value,
                    Url = $"{baseUrl}/api/files/{x.ImageFileId.Value}"
                }
                : null
        });

        var paged = await projectedQuery.ToPagedResultAsync(queryDto, ct);
        return Ok(ProcessResponse<PagedResponse<PersonDto>>.Success(paged));
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcessResponse<PersonDto>>> GetById(int id, CancellationToken ct = default)
    {
        var entity = await _db.Persons
            .Include(p => p.ImageFile) // ← NUEVO
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

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

        // Guardar ID viejo para posible eliminación
        int? oldImageId = entity.ImageFileId;

        MapDtoToEntity(dto, entity);
        await _db.SaveChangesAsync(ct);

        // Si cambió la imagen, borramos la antigua
        if (oldImageId.HasValue && oldImageId != entity.ImageFileId)
        {
            try
            {
                await _fileStorage.DeleteAsync(oldImageId.Value, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar la imagen antigua {Id}", oldImageId);
            }
        }

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

        int? imageId = entity.ImageFileId;

        _db.Persons.Remove(entity);
        await _db.SaveChangesAsync(ct);

        if (imageId.HasValue)
        {
            try { await _fileStorage.DeleteAsync(imageId.Value, ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo eliminar imagen {Id}", imageId); }
        }

        return Ok(ProcessResponse<bool>.Success(true, $"{ENTITY} eliminada correctamente."));
    }

    // ============================================================
    // DELETE BATCH
    // ============================================================
    [HttpPost("batch-delete")]
    public async Task<ActionResult<ProcessResponse<int>>> DeleteBatch(
        [FromBody] BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        List<Person> itemsToDelete;

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

        // Recolectar IDs de imágenes antes de borrar
        var imageIdsToDelete = itemsToDelete
            .Where(p => p.ImageFileId.HasValue)
            .Select(p => p.ImageFileId!.Value)
            .ToList();

        _db.Persons.RemoveRange(itemsToDelete);
        var affectedRows = await _db.SaveChangesAsync(ct);

        // Borrar imágenes del disco
        foreach (var imageId in imageIdsToDelete)
        {
            try { await _fileStorage.DeleteAsync(imageId, ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo eliminar imagen {Id}", imageId); }
        }

        return Ok(ProcessResponse<int>.Success(affectedRows, $"Se eliminaron {affectedRows} {ENTITY.ToLower()}."));
    }

    // ============================================================
    // EXPORT PDF
    // ============================================================
    //[HttpPost("export-pdf")]
    //public async Task<IActionResult> ExportPdfList(
    //[FromBody] BulkSelectionRequest request,
    //CancellationToken ct = default)
    //{
    //    // Obtener configuración (Logo, colores, textos)
    //    var config = await _reportDataProvider.GetConfigAsync(ct);

    //    // Obtener datos del negocio
    //    var query = _db.Persons

    //        .AsNoTracking();

    //    if (!request.SelectAll && request.Ids?.Any() == true)
    //        query = query.Where(c => request.Ids.Contains(c.Id));

    //    var data = await query.ToListAsync(ct);
    //    var dtos = data.Select(MapEntityToDto).ToList();

    //    // Generar PDF directo (Sin IPdfService)
    //    var document = new PersonReportPdf(dtos, config);
    //    var bytes = document.GeneratePdf();

    //    // Nombre de archivo profesional
    //    var fileName = $"{config.ShortName ?? "ANIR"}_Empresas_{DateTime.Now:yyyyMMdd}.pdf";

    //    return File(bytes, "application/pdf", fileName);
    //}


    // ============================================================
    // EXPORT EXCEL
    // ============================================================
    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcel([FromBody] BulkSelectionRequest request, CancellationToken ct = default)
    {
        var query = _db.Persons.Include(p => p.ImageFile).AsNoTracking();

        if (request.Ids is { Count: > 0 })
            query = query.Where(c => request.Ids.Contains(c.Id));

        // CORREGIDO: Traer a memoria primero, luego mapear
        var items = await query.ToListAsync(ct);
        var dtos = items.Select(c => MapEntityToDto(c)).ToList();

        var excelBytes = _excelService.GeneratePersonsExcel(dtos);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "PersonsReport.xlsx");
    }
}