using Anir.Shared.Enums;

namespace Anir.Data.Entities
{
    /// <summary>
    /// Represents an ANIR work carried out by a company.
    /// Contains administrative, economic, and documentary information,
    /// as well as relations with people and associated presentations.
    /// </summary>
    public class AnirWork
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string AnirNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        // Replaced IsPaid with two boolean fields
        public bool HasSocialEffect { get; set; }
        public bool HasEconomicEffect { get; set; }

        // Replaced IsGeneralized with an enum
        public GeneralizationStatus Generalization { get; set; } = GeneralizationStatus.No;

        public decimal EconomicImpact { get; set; }
        public string? Recommendations { get; set; }
        public string? ResolutionNumber { get; set; }
        public string? ImageId { get; set; }
        public string? PdfId { get; set; }

        /// <summary>
        /// Owning company of the work.
        /// </summary>
        public Company Company { get; set; } = null!;

        /// <summary>
        /// People associated with the work.
        /// </summary>
        public ICollection<AnirWorkPerson> AnirWorkPersons { get; set; } = new List<AnirWorkPerson>();

        /// <summary>
        /// Presentations associated with the work.
        /// </summary>
        public ICollection<AnirWorkPresentation> AnirWorkPresentations { get; set; } = new List<AnirWorkPresentation>();
    }

}
