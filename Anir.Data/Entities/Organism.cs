using Microsoft.EntityFrameworkCore;

namespace Anir.Data.Entities
{
    public class Organism
    {
        public int Id { get; set; }

        /// Código DUINE: 00113, 00102, etc.
        public string Code { get; set; } = string.Empty;

        /// Sigla: MINAL, MINCEX, MINSAP…
        public string ShortName { get; set; } = string.Empty;

        /// Nombre completo del organismo
        public string Name { get; set; } = string.Empty;

        /// Empresas subordinadas a este organismo
        public ICollection<Company>? Companies { get; set; }
    }
}
