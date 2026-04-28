// Anir.Api\Controllers\SystemSettingController.cs
using Anir.Application.Common.Interfaces;
using Anir.Data;
using Anir.Data.Entities;
using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Anir.Api.Controllers.Administration;

[ApiController]
[Route("api/[controller]")]
public class SystemSettingController : ControllerBase
{
    private const string ENTITY = "Configuración del sistema";

    private readonly ApplicationDbContext _db;
    private readonly ILogger<SystemSettingController> _logger;
    private readonly IFileStorageService _fileStorage;

    public SystemSettingController(
        ApplicationDbContext db,
        ILogger<SystemSettingController> logger,
        IFileStorageService fileStorage)
    {
        _db = db;
        _logger = logger;
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

    private static void MapDtoToEntity(SystemSettingDto dto, SystemSetting entity)
    {
        entity.ImageFileId = dto.ImageFile?.Id;
        entity.Name = dto.Name;
        entity.ShortName = dto.ShortName;
        entity.TaxId = dto.TaxId;
        entity.LegalRepresentative = dto.LegalRepresentative;
        entity.LegalRepresentativeTitle = dto.LegalRepresentativeTitle;
        entity.Address = dto.Address;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Website = dto.Website;
        entity.ReportHeaderText = dto.ReportHeaderText;
        entity.ReportFooterText = dto.ReportFooterText;
        entity.PrimaryColor = dto.PrimaryColor;
        entity.BackupToolPath = dto.BackupToolPath;
        entity.MaxBackupFiles = dto.MaxBackupFiles;
        entity.AutoBackupEnabled = dto.AutoBackupEnabled;
        entity.AutoMaintenanceEnabled = dto.AutoMaintenanceEnabled;
        entity.ScheduledTime = dto.ScheduledTime ?? TimeSpan.FromHours(2);
    }

    private SystemSettingDto MapEntityToDto(SystemSetting entity) => new()
    {
        Id = entity.Id,
        ImageFile = MapFileResponse(entity.ImageFile),
        Name = entity.Name,
        ShortName = entity.ShortName,
        TaxId = entity.TaxId,
        LegalRepresentative = entity.LegalRepresentative,
        LegalRepresentativeTitle = entity.LegalRepresentativeTitle,
        Address = entity.Address,
        Phone = entity.Phone,
        Email = entity.Email,
        Website = entity.Website,
        ReportHeaderText = entity.ReportHeaderText,
        ReportFooterText = entity.ReportFooterText,
        PrimaryColor = entity.PrimaryColor,
        BackupToolPath = entity.BackupToolPath,
        MaxBackupFiles = entity.MaxBackupFiles,
        LastUpdated = entity.LastUpdated,
        ScheduledTime = entity.ScheduledTime,
             
        AutoBackupEnabled = entity.AutoBackupEnabled,
        AutoMaintenanceEnabled = entity.AutoMaintenanceEnabled
    };

    // ============================================================
    // GET (Singleton)
    // ============================================================
    [HttpGet]
    public async Task<ActionResult<ProcessResponse<SystemSettingDto>>> Get(CancellationToken ct = default)
    {
        var entity = await _db.SystemSettings
            .Include(s => s.ImageFile)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            return NotFound(ProcessResponse<SystemSettingDto>.Fail($"{ENTITY} no encontrada. Ejecute el seeder."));

        var dto = MapEntityToDto(entity);
        return Ok(ProcessResponse<SystemSettingDto>.Success(dto));
    }

    // ============================================================
    // PUT (Singleton)
    // ============================================================
    [HttpPut]
    public async Task<ActionResult<ProcessResponse<SystemSettingDto>>> Update(
        [FromBody] SystemSettingDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ProcessResponse<SystemSettingDto>.Fail("Datos inválidos."));

        var entity = await _db.SystemSettings
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            return NotFound(ProcessResponse<SystemSettingDto>.Fail($"{ENTITY} no encontrada."));

        int? oldImageId = entity.ImageFileId;

        MapDtoToEntity(dto, entity);
        entity.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // Si cambió el logo, eliminar el anterior
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

        // Recargar con include para mapear el archivo
        await _db.Entry(entity).Reference(s => s.ImageFile).LoadAsync(ct);
        var updatedDto = MapEntityToDto(entity);

        return Ok(ProcessResponse<SystemSettingDto>.Success(updatedDto, $"{ENTITY} actualizada correctamente."));
    }
}