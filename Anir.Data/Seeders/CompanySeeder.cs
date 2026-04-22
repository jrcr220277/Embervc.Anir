using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anir.Data.Seeders
{
    public static class CompanySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Companies.Any())
                return;

            var companies = new List<Company>
            {
                new()
                {
                    Id = 1,
                    Code = "01663",
                    ShortName = "EBR-HAB",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS LA HABANA",
                    Address = "SANTA CATALINA NO. 930 E/ PALATINO Y ZUZARTE, REPARTO PALATINO",
                    MunicipalityId = 67,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 2,
                    Code = "01665",
                    ShortName = "EBR-VC",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS VILLA CLARA",
                    Address = "AVENIDA ROJAS NO. 23 ESQUINA A 1RA, REPARTO VIRGINIA",
                    MunicipalityId = 142,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 3,
                    Code = "01666",
                    ShortName = "EBR-CMG",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS CAMAGÜEY",
                    Address = "AVE. DE LA LIBERTAD NO. 159 E/ ARRIETA Y PANAGA, RPTO. AGRAMONTE",
                    MunicipalityId = 12,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 4,
                    Code = "01667",
                    ShortName = "EBR-GRA",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS GRANMA",
                    Address = "CALLE AUGUSTO MÁRQUEZ NO. 24 ENTRE CALLE GENERAL GARCÍA Y AVENIDA GRANMA",
                    MunicipalityId = 35,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 5,
                    Code = "01668",
                    ShortName = "EBR-SCU",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS SANTIAGO DE CUBA",
                    Address = "GARZON NO. 359 E/ AVE. CÉSPEDES Y CALLE ATA",
                    MunicipalityId = 133,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 6,
                    Code = "01674",
                    ShortName = "EBR-PRI",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS PINAR DEL RÍO",
                    Address = "AGRAMONTE FINAL S/N E/ GRAL. LORES Y GONZALEZ ALCORTA",
                    MunicipalityId = 114,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 7,
                    Code = "01679",
                    ShortName = "EBR-CAV",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS CIEGO DE ÁVILA",
                    Address = "CARRETERA CENTRAL KM 468 OESTE",
                    MunicipalityId = 25,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 8,
                    Code = "04796",
                    ShortName = "EBR-MAY",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS MAYABEQUE",
                    Address = "ARMENTEROS NO. 18 ESQ. A CALZADA DE LUYANO",
                    MunicipalityId = 103,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 9,
                    Code = "15185",
                    ShortName = "EBR-GTM",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS GUANTÁNAMO",
                    Address = "CALLE 17 SUR ENTRE 4 Y 5",
                    MunicipalityId = 42,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 10,
                    Code = "15186",
                    ShortName = "EBR-LTU",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS LAS TUNAS",
                    Address = "CALLE 23, ESPINOSA, S/N Y ENTRE 17 Y 19",
                    MunicipalityId = 82,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 11,
                    Code = "15823",
                    ShortName = "EBR-CFG",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS CIENFUEGOS",
                    Address = "CALLE 23 NO. 5619 ENTRE AVENIDAS 56 Y 58",
                    MunicipalityId = 35,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 12,
                    Code = "15824",
                    ShortName = "EBR-HOL",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS HOLGUÍN",
                    Address = "CRISTINO NARANJO NO. 2 Y FINAL REPARTO CIUDAD JARDÍN",
                    MunicipalityId = 52,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 13,
                    Code = "15881",
                    ShortName = "EBR-SSP",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS SANCTI SPÍRITUS",
                    Address = "CALLE BARTOLOMÉ MASSÓ NO. 263 ENTRE SANTA ELENA Y MAYÍA",
                    MunicipalityId = 125,
                    OrganismId = 3,
                    Active = true
                },
                new()
                {
                    Id = 14,
                    Code = "15882",
                    ShortName = "EBR-MTZ",
                    Name = "EMPRESA DE BEBIDAS Y REFRESCOS MATANZAS",
                    Address = "CALLE RÍO NO. 62, ENTRE SANTA TERESA Y ZARAGOZA",
                    MunicipalityId = 90,
                    OrganismId = 3,
                    Active = true
                }
            };

            context.Companies.AddRange(companies);
            await context.SaveChangesAsync();
           
        }
    }


}

