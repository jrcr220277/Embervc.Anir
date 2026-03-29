using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anir.Data.Seeders
{
    /// <summary>
    /// Seeder para cargar las empresas iniciales del sistema ANIR.
    /// Se ejecuta solo si la tabla está vacía.
    /// </summary>
    public static class CompanySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Companies.Any())
                return;

            var companies = new List<Company>
            {
                new() { ShortName = "Ueb02", Name = "UEB Aseguramiento",
                    Address = "Calle Tirso Díaz No. 136 Esquina Carretera Central Reparto Virginia Santa Clara",
                    MunicipalityId = 142, Active = true },

                new() { ShortName = "Ueb03", Name = "UEB Embotelladora Amaro",
                    Address = "Carretera Sitio Grande No. 3 Amaro Santo Domingo VC",
                    MunicipalityId = 153, Active = true },

                new() { ShortName = "Ueb04", Name = "Estación Distribuidora Santo Domingo",
                    Address = "Calle Eustaquio Delgado No. 104 e/ Colón y Juan B. Zayas Santo Domingo VC",
                    MunicipalityId = 153, Active = true },

                new() { ShortName = "Ueb05", Name = "Estación Distribuidora Sagua",
                    Address = "Calle Quirós s/n e/ Flor Crombet y Quintín Banderas Sagua la Grande VC",
                    MunicipalityId = 152, Active = true },

                new() { ShortName = "Ueb06", Name = "UEB Embotelladora Calabazar de Sagua",
                    Address = "Carretera Encrucijada Km 1 Calabazar de Sagua VC",
                    MunicipalityId = 147, Active = true },

                new() { ShortName = "Ueb07", Name = "UEB Combinado Cubanacán",
                    Address = "Valeriano López No. 16 e/Hermanos Cárdenas y Marino Cabrera Camajuaní VC",
                    MunicipalityId = 144, Active = true },

                new() { ShortName = "Ueb08", Name = "UEB Vinatera del Norte",
                    Address = "Calle 4 No. 103 e/ 1 y 5 Caibarién VC",
                    MunicipalityId = 143, Active = true },

                new() { ShortName = "Ueb09", Name = "Estación Distribuidora Placetas",
                    Address = "Calle 2da del Oeste No. 38 e/ 1ra y 2da del norte Placetas VC",
                    MunicipalityId = 149, Active = true },

                new() { ShortName = "Ueb10", Name = "UEB Embotelladora Central",
                    Address = "Carretera Acueducto y Ave 26 de Julio Santa Clara VC",
                    MunicipalityId = 142, Active = true },

                new() { ShortName = "Ueb11", Name = "UEB Comercializadora",
                    Address = "Calle 1ra y Nueva Reparto Virginia Santa Clara VC",
                    MunicipalityId = 142, Active = true },

                new() { ShortName = "Ueb12", Name = "UEB Transporte",
                    Address = "Carretera Central Km 295 Crucero de Vila Reparto Manuelita Santa Clara VC",
                    MunicipalityId = 142, Active = true },

                new() { ShortName = "Ueb13", Name = "Estación Distribuidora Ranchuelo",
                    Address = "Calle Camilo Cienfuegos No. 1 Ranchuelo VC",
                    MunicipalityId = 151, Active = true },

                new() { ShortName = "Ueb14", Name = "Estación Distribuidora Manicaragua",
                    Address = "Circunvalación s/n Manicaragua VC",
                    MunicipalityId = 148, Active = true },

                new() { ShortName = "Ueb15", Name = "Direccion de Empresa",
                    Address = "Avenida Rojas e/ a",
                    MunicipalityId = 142, Active = true }
            };

            context.Companies.AddRange(companies);
            await context.SaveChangesAsync();

            // Ajustar la secuencia UNA sola vez
            await context.Database.ExecuteSqlRawAsync(
                "SELECT setval(pg_get_serial_sequence('\"Companies\"', 'Id'), (SELECT MAX(\"Id\") FROM \"Companies\"))");
        }
    }
}
