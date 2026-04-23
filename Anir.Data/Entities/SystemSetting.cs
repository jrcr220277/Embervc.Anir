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

    // ── Meta ───────────────────────────────────
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}