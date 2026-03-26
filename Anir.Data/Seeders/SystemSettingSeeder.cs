using Anir.Data.Entities;

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
                    Id = 1,
                    LogoId = "App-logo.png",
                    Name = "Empresa de Bebidas y Refrescos VC",
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
