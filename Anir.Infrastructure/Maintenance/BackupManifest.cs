// Anir.Infrastructure\Maintenance\BackupManifest.cs
using System.Text.Json.Serialization;

namespace Anir.Infrastructure.Maintenance;

public class BackupManifest
{
    // Constante única. Si es un backup de ANIR, siempre será esto. NUNCA cambia.
    public const string SystemSignature = "ANIR_SYSTEM_BACKUP";

    [JsonPropertyName("system_id")]
    public string SystemId { get; set; } = SystemSignature;

    // Versión del ensamblado de la API (se lee automáticamente)
    [JsonPropertyName("app_version")]
    public string AppVersion { get; set; } = string.Empty;

    // La última migración de EF Core (La verdad absoluta de la estructura de la BD)
    [JsonPropertyName("last_migration")]
    public string LastMigration { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}