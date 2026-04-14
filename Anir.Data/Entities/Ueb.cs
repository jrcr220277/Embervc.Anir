namespace Anir.Data.Entities
{
    public class Ueb
    {
        public int Id { get; set; }

        /// <summary>
        /// Código DUINE de la UEB (ej: 01663).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo de la UEB.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public int? MunicipalityId { get; set; }
        public Municipality? Municipality { get; set; }

        /// <summary>
        /// Empresa a la que pertenece.
        /// </summary>
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        /// <summary>
        /// Trabajos ANIR asociados a la empresa.
        /// </summary>
        public ICollection<AnirWork>? AnirWorks { get; set; }

        /// <summary>
        /// Estado de la UEB.
        /// </summary>
        public bool Active { get; set; } = true;
    }
}
