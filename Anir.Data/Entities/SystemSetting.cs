// Anir.Data\Entities\SystemSetting.cs
namespace Anir.Data.Entities;

public class SystemSetting
{
    public int Id { get; set; }

    // ── Identidad ──────────────────────────────
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }

    public int? ImageFileId { get; set; }
    public StoredFile? ImageFile { get; set; }

    public string? TaxId { get; set; }
    public string? LegalRepresentative { get; set; }
    public string? LegalRepresentativeTitle { get; set; }

    // ── Contacto ───────────────────────────────
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    // ── Reportes ───────────────────────────────
    public string? ReportHeaderText { get; set; }
    public string? ReportFooterText { get; set; }

    // ── Branding ───────────────────────────────
    public string? PrimaryColor { get; set; }

    // ── Respaldos ───────────────────────────────
    public string? BackupToolPath { get; set; } // Ej: "C:\Program Files\PostgreSQL\16\bin\pg_dump.exe"
    public int MaxBackupFiles { get; set; } = 5;

    // ── Automatización ───────────────────────────────
    public bool AutoBackupEnabled { get; set; } = false;
    public bool AutoMaintenanceEnabled { get; set; } = false;

    // Hora única para las tareas automáticas (Formato 24h)
    public TimeSpan ScheduledTime { get; set; } = new TimeSpan(2, 0, 0); // 2:00 AM por defecto

    // ── Meta ───────────────────────────────────
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}