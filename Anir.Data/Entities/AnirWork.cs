namespace Anir.Data.Entities
{
    /// <summary>
    /// Representa un trabajo ANIR realizado por una empresa.
    /// Contiene información administrativa, económica y documental,
    /// además de relaciones con personas y presentaciones asociadas.
    /// </summary>
    public class AnirWork
    {
        public int Id { get; set; }

        /// <summary>
        /// Empresa a la que pertenece el trabajo.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Fecha en que se registró o realizó el trabajo.
        /// </summary>
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        /// <summary>
        /// Número identificador del trabajo ANIR.
        /// </summary>
        public string AnirNumber { get; set; } = null!;

        /// <summary>
        /// Título o nombre del trabajo.
        /// </summary>
        public string Title { get; set; } = null!;

        public string? Description { get; set; }
        public bool IsPaid { get; set; }
        public bool IsGeneralized { get; set; }
        public decimal EconomicImpact { get; set; }
        public string? Recommendations { get; set; }
        public string? ResolutionNumber { get; set; }

        /// <summary>
        /// Identificador de la imagen asociada al trabajo.
        /// </summary>
        public string? ImageId { get; set; }

        /// <summary>
        /// Identificador del PDF asociado al trabajo.
        /// </summary>
        public string? PdfId { get; set; }

        /// <summary>
        /// Empresa propietaria del trabajo.
        /// </summary>
        public Company Company { get; set; } = null!;

        /// <summary>
        /// Personas asociadas al trabajo.
        /// </summary>
        public ICollection<AnirWorkPerson> AnirWorkPersons { get; set; } = new List<AnirWorkPerson>();

        /// <summary>
        /// Presentaciones asociadas al trabajo.
        /// </summary>
        public ICollection<AnirWorkPresentation> AnirWorkPresentations { get; set; } = new List<AnirWorkPresentation>();
    }
}
