using Anir.Shared.Enums;

namespace Anir.Data.Entities
{
    /// <summary>
    /// Represents an ANIR work carried out by a specific UEB.
    /// The UEB determines the Company and Organism through navigation.
    /// </summary>
    public class AnirWork
    {
        public int Id { get; set; }

        // ============================
        // RELACIÓN ORGANIZATIVA REAL
        // ============================
        public int UebId { get; set; }
        public Ueb Ueb { get; set; } = null!;

        // ============================
        // DATOS BASE
        // ============================
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string AnirNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        // ============================
        // EFECTOS
        // ============================
        public bool HasSocialEffect { get; set; }
        public bool HasEconomicEffect { get; set; }
        public JobCategory Category { get; set; } = JobCategory.Innovacion;
        public GeneralizationStatus Generalization { get; set; } = GeneralizationStatus.No;
        public bool IsExperimental { get; set; }
        public DateTime? ExperimentalStartDate { get; set; }
        public DateTime? ExperimentalEndDate { get; set; }

        // ============================
        // ECONOMÍA
        // ============================
        public decimal EconomicImpact { get; set; }
        public string? Recommendations { get; set; }
        public JobState State { get; set; } = JobState.Aprobado;
        public string? ResolutionNumber { get; set; }

        // ============================
        // ARCHIVOS
        // ============================
        public string? ImageId { get; set; }
        public string? PdfId { get; set; }

        // ============================
        // RELACIONES
        // ============================
        public ICollection<AnirWorkPerson> AnirWorkPersons { get; set; } = new List<AnirWorkPerson>();
        public ICollection<AnirWorkPresentation> AnirWorkPresentations { get; set; } = new List<AnirWorkPresentation>();

    }
}
