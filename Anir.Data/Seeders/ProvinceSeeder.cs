using Anir.Data.Entities;

namespace Anir.Data.Seeders
{
    public static class ProvinceSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Provinces.Any()) return;

            var provinces = new List<Province>
            {
                new() { Id = 1, Name = "Artemisa", ShortName = "ART" },
                new() { Id = 2, Name = "Camagüey", ShortName = "CMG" },
                new() { Id = 3, Name = "Ciego de Ávila", ShortName = "CAV" },
                new() { Id = 4, Name = "Cienfuegos", ShortName = "CFG" },
                new() { Id = 5, Name = "Granma", ShortName = "GRM" },
                new() { Id = 6, Name = "Guantánamo", ShortName = "GTM" },
                new() { Id = 7, Name = "Holguín", ShortName = "HOL" },
                new() { Id = 8, Name = "La Habana", ShortName = "LHA", IsCapital = true },
                new() { Id = 9, Name = "Las Tunas", ShortName = "LTN" },
                new() { Id = 10, Name = "Matanzas", ShortName = "MTZ" },
                new() { Id = 11, Name = "Mayabeque", ShortName = "MAY" },
                new() { Id = 12, Name = "Pinar del Río", ShortName = "PNR" },
                new() { Id = 13, Name = "Sancti Spíritus", ShortName = "SSP" },
                new() { Id = 14, Name = "Santiago de Cuba", ShortName = "SCU" },
                new() { Id = 15, Name = "Villa Clara", ShortName = "VCL" },
                new() { Id = 16, Name = "Isla de la Juventud", ShortName = "ILJ" }
            };

            context.Provinces.AddRange(provinces);
            await context.SaveChangesAsync();
        }
    }
}
