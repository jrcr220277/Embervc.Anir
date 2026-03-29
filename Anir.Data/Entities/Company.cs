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

       
        /// <summary>
        /// Identificador del municipio al que pertenece la empresa.
        /// </summary>
        public int? MunicipalityId { get; set; }


        /// <summary>
        /// Indica si la empresa está activa en el sistema.
        /// </summary>
        public bool Active { get; set; } = true;



        /// <summary>
        /// Municipio al que pertenece la empresa.
        /// </summary>
        public Municipality? Municipality { get; set; }

        /// <summary>
        /// Trabajos ANIR asociados a la empresa.
        /// </summary>
        public ICollection<AnirWork>? AnirWorks { get; set; }
    }
}
