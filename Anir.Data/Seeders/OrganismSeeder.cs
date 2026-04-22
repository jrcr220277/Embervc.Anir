using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anir.Data.Seeders
{
    public static class OrganismSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Organisms.Any())
                return;

            var organisms = new List<Organism>
            {
                new() { Id = 1, Code = "00102", ShortName = "MINDUS", Name = "MINISTERIO DE INDUSTRIAS" },
                new() { Id = 2, Code = "00104", ShortName = "MINEM", Name = "MINISTERIO DE ENERGIA Y MINAS" },
                new() { Id = 3, Code = "00113", ShortName = "MINAL", Name = "MINISTERIO DE LA INDUSTRIA ALIMENTARIA" },
                new() { Id = 4, Code = "00118", ShortName = "INRH", Name = "INSTITUTO NACIONAL DE RECURSOS HIDRAULICOS" },
                new() { Id = 5, Code = "00123", ShortName = "MICONS", Name = "MINISTERIO DE LA CONSTRUCCION" },
                new() { Id = 6, Code = "00131", ShortName = "MINAG", Name = "MINISTERIO DE LA AGRICULTURA" },
                new() { Id = 7, Code = "00151", ShortName = "MITRANS", Name = "MINISTERIO DEL TRANSPORTE" },
                new() { Id = 8, Code = "00161", ShortName = "MINCOM", Name = "MINISTERIO DE COMUNICACIONES" },
                new() { Id = 9, Code = "00174", ShortName = "MINCIN", Name = "MINISTERIO DEL COMERCIO INTERIOR" },
                new() { Id = 10, Code = "00177", ShortName = "MINCEX", Name = "MINISTERIO DEL COMERCIO EXTERIOR Y LA INVERSION EXTRANJERA" },
                new() { Id = 11, Code = "00221", ShortName = "CITMA", Name = "MINISTERIO DE CIENCIA, TECNOLOGIA Y MEDIO AMBIENTE" },
                new() { Id = 12, Code = "00223", ShortName = "MINED", Name = "MINISTERIO DE EDUCACION" },
                new() { Id = 13, Code = "00224", ShortName = "MES", Name = "MINISTERIO DE EDUCACION SUPERIOR" },
                new() { Id = 14, Code = "00234", ShortName = "MINCULT", Name = "MINISTERIO DE CULTURA" },
                new() { Id = 15, Code = "00241", ShortName = "MINSAP", Name = "MINISTERIO DE SALUD PUBLICA" },
                new() { Id = 16, Code = "00242", ShortName = "INDER", Name = "INSTITUTO NACIONAL DE DEPORTES, EDUCACION FISICA Y RECREACION" },
                new() { Id = 17, Code = "00254", ShortName = "MFP", Name = "MINISTERIO DE FINANZAS Y PRECIOS" },
                new() { Id = 18, Code = "00255", ShortName = "MINTUR", Name = "MINISTERIO DE TURISMO" },
                new() { Id = 19, Code = "00261", ShortName = "MEP", Name = "MINISTERIO DE ECONOMIA Y PLANIFICACION" },
                new() { Id = 20, Code = "00262", ShortName = "MTSS", Name = "MINISTERIO DE TRABAJO Y SEGURIDAD SOCIAL" },
                new() { Id = 21, Code = "00263", ShortName = "MINJUS", Name = "MINISTERIO DE JUSTICIA" },
                new() { Id = 22, Code = "00264", ShortName = "MINREX", Name = "MINISTERIO DE RELACIONES EXTERIORES" },
                new() { Id = 23, Code = "00271", ShortName = "MINFAR", Name = "MINISTERIO DE LAS FUERZAS ARMADAS REVOLUCIONARIAS" },
                new() { Id = 24, Code = "00272", ShortName = "MININT", Name = "MINISTERIO DEL INTERIOR" },
                new() { Id = 25, Code = "01498", ShortName = "IICS", Name = "INSTITUTO DE INFORMACION Y COMUNICACION SOCIAL" },
                new() { Id = 26, Code = "15065", ShortName = "INOTU", Name = "INSTITUTO NACIONAL DE ORDENAMIENTO TERRITORIAL Y URBANISMO" }
            };

            context.Organisms.AddRange(organisms);
            await context.SaveChangesAsync();
                  }
    }
}
