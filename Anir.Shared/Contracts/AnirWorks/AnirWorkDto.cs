using System.ComponentModel.DataAnnotations;
using Anir.Shared.Contracts.AnirWorks.Persons;
using Anir.Shared.Contracts.AnirWorks.Presentations;
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

    [Display(Name = "Empresa")]
    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una UEB.")]
    [Display(Name = "UEB")]
    public int UebId { get; set; }

    [Display(Name = "UEB")]
    public string? UebName { get; set; }

    // ============================================================
    // DATOS BASE
    // ============================================================

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [Display(Name = "Fecha")]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [Required(ErrorMessage = "El número ANIR es obligatorio.")]
    [StringLength(50, ErrorMessage = "El número ANIR no puede exceder {1} caracteres.")]
    [Display(Name = "Número ANIR")]
    public string AnirNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede exceder {1} caracteres.")]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede exceder {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    // ============================================================
    // EFECTOS
    // ============================================================

    [Display(Name = "Efecto Social")]
    public bool HasSocialEffect { get; set; }

    [Display(Name = "Efecto Económico")]
    public bool HasEconomicEffect { get; set; }

    [Display(Name = "Generalización")]
    public GeneralizationStatus Generalization { get; set; } = GeneralizationStatus.Pending;

    // ============================================================
    // ECONOMÍA
    // ============================================================

    [Range(0, double.MaxValue, ErrorMessage = "El impacto económico debe ser mayor o igual a 0.")]
    [Display(Name = "Impacto Económico")]
    public decimal EconomicImpact { get; set; }

    [StringLength(2000, ErrorMessage = "Las recomendaciones no pueden exceder {1} caracteres.")]
    [Display(Name = "Recomendaciones")]
    public string? Recommendations { get; set; }

    [StringLength(50, ErrorMessage = "El número de resolución no puede exceder {1} caracteres.")]
    [Display(Name = "Número de Resolución")]
    public string? ResolutionNumber { get; set; }

    // ============================================================
    // ARCHIVOS
    // ============================================================

    public string? ImageId { get; set; }
    public string? ImageUrl { get; set; }

    public string? PdfId { get; set; }
    public string? PdfUrl { get; set; }

    // ============================================================
    // RELACIONES
    // ============================================================

    [Display(Name = "Personas")]
    public List<AnirWorkPersonDto> Persons { get; set; } = new();

    [Display(Name = "Presentaciones")]
    public List<AnirWorkPresentationDto> Presentations { get; set; } = new();
}
