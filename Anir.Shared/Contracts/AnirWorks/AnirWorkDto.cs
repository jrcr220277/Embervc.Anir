using System.ComponentModel.DataAnnotations;
using Anir.Shared.Contracts.AnirWorks.Persons;
using Anir.Shared.Contracts.AnirWorks.Presentations;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Enums;

namespace Anir.Shared.Contracts.AnirWorks;

public class AnirWorkDto
{
    public int Id { get; set; }

    // ============================================================
    // ORGANIZACIÓN
    // ============================================================

    [Required(ErrorMessage = "Debe seleccionar una empresa.")]
    [Display(Name = "Empresa")]
    public int CompanyId { get; set; }

    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una UEB.")]
    [Display(Name = "UEB")]
    public int UebId { get; set; }

    public string? UebName { get; set; }

    // ============================================================
    // DATOS BASE
    // ============================================================

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [Display(Name = "Fecha de Registro")]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [Required(ErrorMessage = "El número registro es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Número Registro")]
    public string AnirNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    // ============================================================
    // EFECTOS
    // ============================================================

    public bool HasSocialEffect { get; set; }
    public bool HasEconomicEffect { get; set; }

    public JobCategory Category { get; set; } = JobCategory.Innovacion;
    public GeneralizationStatus Generalization { get; set; } = GeneralizationStatus.No;

    public bool IsExperimental { get; set; }
    public DateTime? ExperimentalStartDate { get; set; }
    public DateTime? ExperimentalEndDate { get; set; }

    // ============================================================
    // ECONOMÍA
    // ============================================================

    [Range(0, double.MaxValue)]
    public decimal EconomicImpact { get; set; }

    [StringLength(2000)]
    public string? Recommendations { get; set; }

    public JobState State { get; set; } = JobState.Aprobado;

    [StringLength(50)]
    public string? ResolutionNumber { get; set; }

    // ============================================================
    // ARCHIVOS
    // ============================================================

    public FileResponse? ImageFile { get; set; }
    public FileResponse? PdfFile { get; set; }

    // ============================================================
    // RELACIONES
    // ============================================================

    public List<AnirWorkPersonDto> Persons { get; set; } = new();
    public List<AnirWorkPresentationDto> Presentations { get; set; } = new();
}