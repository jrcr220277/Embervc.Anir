using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Settings; 
using Anir.Shared.Contracts.Common; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Anir.Infrastructure.Reports;

public class ReportDataProvider : IReportDataProvider
{
    private readonly ApplicationDbContext _db;
    private readonly string _rootPath;
    private ReportConfigDto? _cachedConfig;

    // Inyectamos la misma configuración que usa tu FileStorageService
    public ReportDataProvider(ApplicationDbContext db, IOptions<FileStorageSettings> settings)
    {
        _db = db;

        // Lógica EXACTA que usas en tu FileStorageService para calcular la ruta raíz
        var path = settings.Value.RootPath;
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), path);
        }

        _rootPath = path;
    }

    public async Task<ReportConfigDto> GetConfigAsync(CancellationToken ct = default)
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        var settings = await _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(ct);

        var config = new ReportConfigDto
        {
            CompanyName = settings?.Name ?? "ANIR",
            ShortName = settings?.ShortName,
            HeaderText = settings?.ReportHeaderText,
            FooterText = settings?.ReportFooterText,
            PrimaryColor = settings?.PrimaryColor ?? "#4A6FA5"
        };

        if (settings?.ImageFileId.HasValue == true)
        {
            var file = await _db.StoredFiles.AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == settings.ImageFileId.Value, ct);

            // Reconstruimos la ruta exacta igual que tu FileStorageService
            if (file != null)
            {
                var physicalPath = Path.Combine(_rootPath, file.Folder, file.FileName);

                if (System.IO.File.Exists(physicalPath))
                {
                    config.LogoBytes = await System.IO.File.ReadAllBytesAsync(physicalPath, ct);
                }
            }
        }

        _cachedConfig = config;
        return config;
    }
}