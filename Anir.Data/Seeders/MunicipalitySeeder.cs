using Anir.Data.Entities;

namespace Anir.Data.Seeders
{
    /// <summary>
    /// Seeder para cargar todos los municipios de Cuba.
    /// Se ejecuta solo si la tabla está vacía.
    /// </summary>
    public static class MunicipalitySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Municipalities.Any())
                return;

            var municipalities = new List<Municipality>
            {
                // Artemisa
                new() { Id = 1, Name = "Alquizar", ProvinceId = 1 },
                new() { Id = 2, Name = "Artemisa", ProvinceId = 1, IsProvinceCapital = true },
                new() { Id = 3, Name = "Bauta", ProvinceId = 1 },
                new() { Id = 4, Name = "Caimito", ProvinceId = 1 },
                new() { Id = 5, Name = "Guanajay", ProvinceId = 1 },
                new() { Id = 6, Name = "Güira de Melena", ProvinceId = 1 },
                new() { Id = 7, Name = "Mariel", ProvinceId = 1 },
                new() { Id = 8, Name = "San Antonio de los Baños", ProvinceId = 1 },
                new() { Id = 9, Name = "Bahía Honda", ProvinceId = 1 },
                new() { Id = 10, Name = "San Cristóbal", ProvinceId = 1 },
                new() { Id = 11, Name = "Candelaria", ProvinceId = 1 },

                // Camagüey
                new() { Id = 12, Name = "Camagüey", ProvinceId = 2, IsProvinceCapital = true },
                new() { Id = 13, Name = "Carlos Manuel de Céspedes", ProvinceId = 2 },
                new() { Id = 14, Name = "Esmeralda", ProvinceId = 2 },
                new() { Id = 15, Name = "Florida", ProvinceId = 2 },
                new() { Id = 16, Name = "Guaimaro", ProvinceId = 2 },
                new() { Id = 17, Name = "Jimagüayú", ProvinceId = 2 },
                new() { Id = 18, Name = "Minas", ProvinceId = 2 },
                new() { Id = 19, Name = "Najasa", ProvinceId = 2 },
                new() { Id = 20, Name = "Nuevitas", ProvinceId = 2 },
                new() { Id = 21, Name = "Santa Cruz del Sur", ProvinceId = 2 },
                new() { Id = 22, Name = "Sibanicú", ProvinceId = 2 },
                new() { Id = 23, Name = "Sierra de Cubitas", ProvinceId = 2 },
                new() { Id = 24, Name = "Vertientes", ProvinceId = 2 },

                // Ciego de Ávila
                new() { Id = 25, Name = "Ciego de Ávila", ProvinceId = 3, IsProvinceCapital = true },
                new() { Id = 26, Name = "Ciro Redondo", ProvinceId = 3 },
                new() { Id = 27, Name = "Baraguá", ProvinceId = 3 },
                new() { Id = 28, Name = "Bolivia", ProvinceId = 3 },
                new() { Id = 29, Name = "Chambas", ProvinceId = 3 },
                new() { Id = 30, Name = "Florencia", ProvinceId = 3 },
                new() { Id = 31, Name = "Majagua", ProvinceId = 3 },
                new() { Id = 32, Name = "Morón", ProvinceId = 3 },
                new() { Id = 33, Name = "Primero de Enero", ProvinceId = 3 },
                new() { Id = 34, Name = "Venezuela", ProvinceId = 3 },

                // Cienfuegos
                new() { Id = 35, Name = "Cienfuegos", ProvinceId = 4, IsProvinceCapital = true },
                new() { Id = 36, Name = "Aguada de Pasajeros", ProvinceId = 4 },
                new() { Id = 37, Name = "Cruces", ProvinceId = 4 },
                new() { Id = 38, Name = "Cumanayagua", ProvinceId = 4 },
                new() { Id = 39, Name = "Palmira", ProvinceId = 4 },
                new() { Id = 40, Name = "Rodas", ProvinceId = 4 },
                new() { Id = 41, Name = "Santa Isabel de las Lajas", ProvinceId = 4 },

                // Guantánamo
                new() { Id = 42, Name = "Guantánamo", ProvinceId = 6, IsProvinceCapital = true },
                new() { Id = 43, Name = "Baracoa", ProvinceId = 6 },
                new() { Id = 44, Name = "Caimanera", ProvinceId = 6 },
                new() { Id = 45, Name = "El Salvador", ProvinceId = 6 },
                new() { Id = 46, Name = "Imías", ProvinceId = 6 },
                new() { Id = 47, Name = "Maisí", ProvinceId = 6 },
                new() { Id = 48, Name = "Manuel Tames", ProvinceId = 6 },
                new() { Id = 49, Name = "Niceto Pérez", ProvinceId = 6 },
                new() { Id = 50, Name = "San Antonio del Sur", ProvinceId = 6 },
                new() { Id = 51, Name = "Yateras", ProvinceId = 6 },

                // Holguín
                new() { Id = 52, Name = "Holguín", ProvinceId = 7, IsProvinceCapital = true },
                new() { Id = 53, Name = "Antilla", ProvinceId = 7 },
                new() { Id = 54, Name = "Báguanos", ProvinceId = 7 },
                new() { Id = 55, Name = "Banes", ProvinceId = 7 },
                new() { Id = 56, Name = "Cacocum", ProvinceId = 7 },
                new() { Id = 57, Name = "Calixto García", ProvinceId = 7 },
                new() { Id = 58, Name = "Cueto", ProvinceId = 7 },
                new() { Id = 59, Name = "Frank País", ProvinceId = 7 },
                new() { Id = 60, Name = "Gibara", ProvinceId = 7 },
                new() { Id = 61, Name = "Mayarí", ProvinceId = 7 },
                new() { Id = 62, Name = "Moa", ProvinceId = 7 },
                new() { Id = 63, Name = "Rafael Freyre", ProvinceId = 7 },
                new() { Id = 64, Name = "Sagua de Tánamo", ProvinceId = 7 },
                new() { Id = 65, Name = "Urbano Noris", ProvinceId = 7 },

                // La Habana
                new() { Id = 67, Name = "Plaza", ProvinceId = 8, IsProvinceCapital = true },
                new() { Id = 68, Name = "Arroyo Naranjo", ProvinceId = 8 },
                new() { Id = 69, Name = "Boyeros", ProvinceId = 8 },
                new() { Id = 70, Name = "Cerro", ProvinceId = 8 },
                new() { Id = 71, Name = "Cotorro", ProvinceId = 8 },
                new() { Id = 72, Name = "Diez de Octubre", ProvinceId = 8 },
                new() { Id = 73, Name = "Guanabacoa", ProvinceId = 8 },
                new() { Id = 74, Name = "Habana del Este", ProvinceId = 8 },
                new() { Id = 75, Name = "Habana Vieja", ProvinceId = 8 },
                new() { Id = 76, Name = "La Lisa", ProvinceId = 8 },
                new() { Id = 77, Name = "Marianao", ProvinceId = 8 },
                new() { Id = 78, Name = "Playa", ProvinceId = 8 },
                new() { Id = 79, Name = "Regla", ProvinceId = 8 },
                new() { Id = 80, Name = "San Miguel del Padrón", ProvinceId = 8 },
                new() { Id = 81, Name = "Centro Habana", ProvinceId = 8 },

                // Las Tunas
                new() { Id = 82, Name = "Las Tunas", ProvinceId = 9, IsProvinceCapital = true },
                new() { Id = 83, Name = "Amancio Rodríguez", ProvinceId = 9 },
                new() { Id = 84, Name = "Colombia", ProvinceId = 9 },
                new() { Id = 85, Name = "Jesús Menéndez", ProvinceId = 9 },
                new() { Id = 86, Name = "Jobabo", ProvinceId = 9 },
                new() { Id = 87, Name = "Majibacoa", ProvinceId = 9 },
                new() { Id = 88, Name = "Manatí", ProvinceId = 9 },
                new() { Id = 89, Name = "Puerto Padre", ProvinceId = 9 },

                // Matanzas
                new() { Id = 90, Name = "Matanzas", ProvinceId = 10, IsProvinceCapital = true },
                new() { Id = 91, Name = "Calimete", ProvinceId = 10 },
                new() { Id = 92, Name = "Cárdenas", ProvinceId = 10 },
                new() { Id = 93, Name = "Ciénaga de Zapata", ProvinceId = 10 },
                new() { Id = 94, Name = "Colón", ProvinceId = 10 },
                new() { Id = 95, Name = "Jagüey Grande", ProvinceId = 10 },
                new() { Id = 96, Name = "Jovellanos", ProvinceId = 10 },
                new() { Id = 97, Name = "Limonar", ProvinceId = 10 },
                new() { Id = 98, Name = "Los Arabos", ProvinceId = 10 },
                new() { Id = 99, Name = "Martí", ProvinceId = 10 },
                new() { Id = 100, Name = "Pedro Betancourt", ProvinceId = 10 },
                new() { Id = 101, Name = "Perico", ProvinceId = 10 },
                new() { Id = 102, Name = "Unión de Reyes", ProvinceId = 10 },

                // Mayabeque
                new() { Id = 103, Name = "San José de las Lajas", ProvinceId = 11, IsProvinceCapital = true },
                new() { Id = 104, Name = "Batabanó", ProvinceId = 11 },
                new() { Id = 105, Name = "Bejucal", ProvinceId = 11 },
                new() { Id = 106, Name = "Güines", ProvinceId = 11 },
                new() { Id = 107, Name = "Jaruco", ProvinceId = 11 },
                new() { Id = 108, Name = "Madruga", ProvinceId = 11 },
                new() { Id = 109, Name = "Melena del Sur", ProvinceId = 11 },
                new() { Id = 110, Name = "Nueva Paz", ProvinceId = 11 },
                new() { Id = 111, Name = "Quivicán", ProvinceId = 11 },
                new() { Id = 112, Name = "San Nicolás de Bari", ProvinceId = 11 },
                new() { Id = 113, Name = "Santa Cruz del Norte", ProvinceId = 11 },

                // Pinar del Río
                new() { Id = 114, Name = "Pinar del Río", ProvinceId = 12, IsProvinceCapital = true },
                new() { Id = 115, Name = "Consolación del Sur", ProvinceId = 12 },
                new() { Id = 116, Name = "Guane", ProvinceId = 12 },
                new() { Id = 117, Name = "La Palma", ProvinceId = 12 },
                new() { Id = 118, Name = "Los Palacios", ProvinceId = 12 },
                new() { Id = 119, Name = "Mantua", ProvinceId = 12 },
                new() { Id = 120, Name = "Minas de Matahambre", ProvinceId = 12 },
                new() { Id = 121, Name = "San Juan y Martínez", ProvinceId = 12 },
                new() { Id = 122, Name = "San Luis", ProvinceId = 12 },
                new() { Id = 123, Name = "Sandino", ProvinceId = 12 },
                new() { Id = 124, Name = "Viñales", ProvinceId = 12 },

                // Sancti Spíritus
                new() { Id = 125, Name = "Sancti Spíritus", ProvinceId = 13, IsProvinceCapital = true },
                new() { Id = 126, Name = "Cabaigúan", ProvinceId = 13 },
                new() { Id = 127, Name = "Fomento", ProvinceId = 13 },
                new() { Id = 128, Name = "Jatibonico", ProvinceId = 13 },
                new() { Id = 129, Name = "La Sierpe", ProvinceId = 13 },
                new() { Id = 130, Name = "Taguasco", ProvinceId = 13 },
                new() { Id = 131, Name = "Trinidad", ProvinceId = 13 },
                new() { Id = 132, Name = "Yaguajay", ProvinceId = 13 },

                // Santiago de Cuba
                new() { Id = 133, Name = "Santiago de Cuba", ProvinceId = 14, IsProvinceCapital = true },
                new() { Id = 134, Name = "Contramaestre", ProvinceId = 14 },
                new() { Id = 135, Name = "Guamá", ProvinceId = 14 },
                new() { Id = 136, Name = "Julio Antonio Mella", ProvinceId = 14 },
                new() { Id = 137, Name = "Palma Soriano", ProvinceId = 14 },
                new() { Id = 138, Name = "San Luis", ProvinceId = 14 },
                new() { Id = 139, Name = "Segundo Frente", ProvinceId = 14 },
                new() { Id = 140, Name = "Songo la Maya", ProvinceId = 14 },
                new() { Id = 141, Name = "Tercer Frente", ProvinceId = 14 },

                // Villa Clara
                new() { Id = 142, Name = "Santa Clara", ProvinceId = 15, IsProvinceCapital = true },
                new() { Id = 143, Name = "Caibarién", ProvinceId = 15 },
                new() { Id = 144, Name = "Camajuaní", ProvinceId = 15 },
                new() { Id = 145, Name = "Cifuentes", ProvinceId = 15 },
                new() { Id = 146, Name = "Corralillo", ProvinceId = 15 },
                new() { Id = 147, Name = "Encrucijada", ProvinceId = 15 },
                new() { Id = 148, Name = "Manicaragua", ProvinceId = 15 },
                new() { Id = 149, Name = "Placetas", ProvinceId = 15 },
                new() { Id = 150, Name = "Quemado de Güines", ProvinceId = 15 },
                new() { Id = 151, Name = "Ranchuelo", ProvinceId = 15 },
                new() { Id = 152, Name = "Sagua la Grande", ProvinceId = 15 },
                new() { Id = 153, Name = "Santo Domingo", ProvinceId = 15 },
                new() { Id = 154, Name = "Remedios", ProvinceId = 15 }
            };

            context.Municipalities.AddRange(municipalities);
            await context.SaveChangesAsync();
        }
    }
}
