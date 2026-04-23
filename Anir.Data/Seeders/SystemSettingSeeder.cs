using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anir.Data.Seeders
{
    public static class SystemSettingSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.SystemSettings.Any())
            {
                context.SystemSettings.Add(new SystemSetting
                {
                    Name = "ANIR",
                    ShortName = "ANIR",
                    PrimaryColor = "#1e88e5",
                    ReportHeaderText = "Sistema de Gestión ANIR",
                    ReportFooterText = "Documento generado automáticamente — No requiere firma",
                    LastUpdated = DateTime.UtcNow,
                    Address = "Avenida de Rojas No.23 esq. 1ra y central, Rpto, Virginia",
                    Phone = "42 208447",
                    Email = "embervc@embervc.alinet.cu",
                    Website = "https://www.embervc.alinet.cu"
                });

                await context.SaveChangesAsync();
            }
        }
    }
}

