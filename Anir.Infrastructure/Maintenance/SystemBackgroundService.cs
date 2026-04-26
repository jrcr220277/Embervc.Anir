// Anir.Infrastructure\Maintenance\SystemBackgroundService.cs
using Anir.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting; // <-- ESTE ES EL QUE TIENE BackgroundService
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Anir.Infrastructure.Maintenance;

public class SystemBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SystemBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(10); // Revisa cada 10 min

    // Para no ejecutarlo dos veces el mismo día
    private DateTime _lastExecutionDate = DateTime.MinValue;

    public SystemBackgroundService(IServiceProvider serviceProvider, ILogger<SystemBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio en segundo plano de ANIR iniciado.");

        using var timer = new PeriodicTimer(_checkInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessScheduledTasksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servicio en segundo plano de ANIR.");
            }
        }
    }

    private async Task ProcessScheduledTasksAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
        var maintenanceService = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();

        var settings = await db.SystemSettings.FirstOrDefaultAsync(ct);
        if (settings == null) return;

        // Si no hay ninguna tarea habilitada, no hace nada
        if (!settings.AutoBackupEnabled && !settings.AutoMaintenanceEnabled) return;

        var now = DateTime.Now;
        var targetTime = settings.ScheduledTime;

        // Verificamos si estamos en la hora correcta Y si ya no se ejecutó hoy
        bool isTimeToRun = now.Hour == targetTime.Hours && now.Minute >= targetTime.Minutes;
        bool alreadyRanToday = _lastExecutionDate.Date == now.Date;

        if (isTimeToRun && !alreadyRanToday)
        {
            _lastExecutionDate = now.Date; // Marcamos el día para no repetir
            _logger.LogInformation("Iniciando tareas programadas para las {Time}...", targetTime.ToString(@"hh\:mm"));

            // ============================================================
            // PASO 1: LIMPIEZA (Si está habilitada) - PRIMERO LA BASURA
            // ============================================================
            if (settings.AutoMaintenanceEnabled)
            {
                try
                {
                    _logger.LogInformation("Ejecutando limpieza automática...");
                    var result = await maintenanceService.CleanOrphansAsync(ct);
                    _logger.LogInformation("Limpieza automática completada: {Summary}", result.Summary);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en limpieza automática.");
                }
            }

            // ============================================================
            // PASO 2: RESPALDO (Si está habilitado) - LUEGO LA FOTO
            // ============================================================
            if (settings.AutoBackupEnabled)
            {
                try
                {
                    _logger.LogInformation("Ejecutando respaldo automático...");
                    var result = await backupService.CreateBackupAsync(ct);
                    _logger.LogInformation("Respaldo automático completado: {FileName}", result.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en respaldo automático.");
                }
            }
        }
    }
}