// Anir.Shared\Contracts\SystemSettings\SystemSettingDto.cs
using System.ComponentModel.DataAnnotations;
using Anir.Shared.Contracts.Common;

namespace Anir.Shared.Contracts.SystemSettings;

public class SystemSettingDto
{
    public int Id { get; set; }

    [Display(Name = "Logo")]
    public FileResponse? ImageFile { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder {1} caracteres.")]
    [Display(Name = "Nombre de la organización")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "El nombre corto no puede exceder {1} caracteres.")]
    [Display(Name = "Nombre corto")]
    public string? ShortName { get; set; }

    [StringLength(50, ErrorMessage = "El identificador fiscal no puede exceder {1} caracteres.")]
    [Display(Name = "Identificación fiscal (NIT/RUC)")]
    public string? TaxId { get; set; }

    [StringLength(150, ErrorMessage = "El nombre no puede exceder {1} caracteres.")]
    [Display(Name = "Representante legal")]
    public string? LegalRepresentative { get; set; }

    [StringLength(100, ErrorMessage = "El cargo no puede exceder {1} caracteres.")]
    [Display(Name = "Cargo del representante")]
    public string? LegalRepresentativeTitle { get; set; }

    [StringLength(300, ErrorMessage = "La dirección no puede exceder {1} caracteres.")]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [StringLength(30, ErrorMessage = "El teléfono no puede exceder {1} caracteres.")]
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder {1} caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string? Email { get; set; }

    [Url(ErrorMessage = "La URL no es válida.")]
    [StringLength(300, ErrorMessage = "La URL no puede exceder {1} caracteres.")]
    [Display(Name = "Sitio web")]
    public string? Website { get; set; }

    [StringLength(500, ErrorMessage = "El texto no puede exceder {1} caracteres.")]
    [Display(Name = "Encabezado de reportes")]
    public string? ReportHeaderText { get; set; }

    [StringLength(500, ErrorMessage = "El texto no puede exceder {1} caracteres.")]
    [Display(Name = "Pie de reportes")]
    public string? ReportFooterText { get; set; }

    [StringLength(7, ErrorMessage = "El color debe estar en formato #RRGGBB.")]
    [Display(Name = "Color primario")]
    public string? PrimaryColor { get; set; }

    [Display(Name = "Última actualización")]
    public DateTime LastUpdated { get; set; }
}