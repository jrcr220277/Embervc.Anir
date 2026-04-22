using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anir.Data.Seeders
{
    public static class UebSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Uebs.Any())
                return;

            var uebs = new List<Ueb>
            {
                new() { Id = 1, Code = "02", Name = "UEB Aseguramiento", Address = "Calle Tirso Díaz No. 136 Esquina Carretera Central Reparto Virginia Santa Clara", MunicipalityId = 142, CompanyId = 2, Active = true },
                new() { Id = 2, Code = "03", Name = "UEB Embotelladora Amaro", Address = "Carretera Sitio Grande No. 3 Amaro Santo Domingo VC", MunicipalityId = 153, CompanyId = 2, Active = true },
                new() { Id = 3, Code = "04", Name = "Estación Distribuidora Santo Domingo", Address = "Calle Eustaquio Delgado No. 104 e/ Colón y Juan B. Zayas Santo Domingo VC", MunicipalityId = 153, CompanyId = 2, Active = true },
                new() { Id = 4, Code = "05", Name = "Estación Distribuidora Sagua", Address = "Calle Quirós s/n e/ Flor Crombet y Quintín Banderas Sagua la Grande VC", MunicipalityId = 152, CompanyId = 2, Active = true },
                new() { Id = 5, Code = "06", Name = "UEB Embotelladora Calabazar de Sagua", Address = "Carretera Encrucijada Km 1 Calabazar de Sagua VC", MunicipalityId = 147, CompanyId = 2, Active = true },
                new() { Id = 6, Code = "07", Name = "UEB Combinado Cubanacán", Address = "Valeriano López No. 16 e/Hermanos Cárdenas y Marino Cabrera Camajuaní VC", MunicipalityId = 144, CompanyId = 2, Active = true },
                new() { Id = 7, Code = "08", Name = "UEB Vinatera del Norte", Address = "Calle 4 No. 103 e/ 1 y 5 Caibarién VC", MunicipalityId = 143, CompanyId = 2, Active = true },
                new() { Id = 8, Code = "09", Name = "Estación Distribuidora Placetas", Address = "Calle 2da del Oeste No. 38 e/ 1ra y 2da del norte Placetas VC", MunicipalityId = 149, CompanyId = 2, Active = true },
                new() { Id = 9, Code = "10", Name = "UEB Embotelladora Central", Address = "Carretera Acueducto y Ave 26 de Julio Santa Clara VC", MunicipalityId = 142, CompanyId = 2, Active = true },
                new() { Id = 10, Code = "11", Name = "UEB Comercializadora", Address = "Calle 1ra y Nueva Reparto Virginia Santa Clara VC", MunicipalityId = 142, CompanyId = 2, Active = true },
                new() { Id = 11, Code = "12", Name = "UEB Transporte", Address = "Carretera Central Km 295 Crucero de Vila Reparto Manuelita Santa Clara VC", MunicipalityId = 142, CompanyId = 2, Active = true },
                new() { Id = 12, Code = "13", Name = "Estación Distribuidora Ranchuelo", Address = "Calle Camilo Cienfuegos No. 1 Ranchuelo VC", MunicipalityId = 151, CompanyId = 2, Active = true },
                new() { Id = 13, Code = "14", Name = "Estación Distribuidora Manicaragua", Address = "Circunvalación s/n Manicaragua VC", MunicipalityId = 148, CompanyId = 2, Active = true },
                new() { Id = 14, Code = "15", Name = "Dirección de Empresa", Address = "Avenida Rojas e/ a", MunicipalityId = 142, CompanyId = 2, Active = true }
            };

            context.Uebs.AddRange(uebs);
            await context.SaveChangesAsync();
                   

        }
    }
}
