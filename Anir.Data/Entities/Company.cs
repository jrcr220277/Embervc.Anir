namespace Anir.Data.Entities
{
    /// <summary>
    /// Representa una empresa registrada en el sistema,
    /// incluyendo su información básica, ubicación y trabajos ANIR asociados.
    /// </summary>
    public class Company
    {
        public int Id { get; set; }

        /// <summary>
        /// Código DUINE de la empresa (ej: 01663).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre corto o abreviado de la empresa.
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo de la empresa.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Dirección física de la empresa.
        /// </summary>
        public string? Address { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }

        /// <summary>
        /// Identificador del municipio al que pertenece la empresa.
        /// </summary>
        public int? MunicipalityId { get; set; }
        public Municipality? Municipality { get; set; }

        /// <summary>
        /// Organismo al que pertenece la empresa.
        /// </summary>
        public int OrganismId { get; set; }
        public Organism Organism { get; set; } = null!;

        /// <summary>
        /// Indica si la empresa está activa en el sistema.
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// Trabajos ANIR asociados a la empresa.
        /// </summary>
        public ICollection<AnirWork>? AnirWorks { get; set; }

        /// <summary>
        /// UEBs subordinadas a esta empresa.
        /// </summary>
        public ICollection<Ueb>? Uebs { get; set; }
    }
}
