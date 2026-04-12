using System.ComponentModel.DataAnnotations;

namespace Anir.Shared.Contracts.Uebs;

public class UebDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código no puede exceder {1} caracteres.")]
    [Display(Name = "Código")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la UEB es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder {1} caracteres.")]
    [Display(Name = "Nombre de la UEB")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La dirección no puede exceder {1} caracteres.")]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder {1} caracteres.")]
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede exceder {1} caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Debe seleccionar la empresa.")]
    [Display(Name = "Empresa")]
    public int CompanyId { get; set; }

    // Solo para mostrar en grilla/detalle
    [Display(Name = "Empresa")]
    public string? CompanyName { get; set; }

    [Display(Name = "Provincia")]
    public string? ProvinceName { get; set; }


    [Display(Name = "Municipio")]
    public int? MunicipalityId { get; set; }

    // Solo para mostrar en grilla/detalle
    [Display(Name = "Municipio")]
    public string? MunicipalityName { get; set; }

    
    [Display(Name = "Activo")]
    public bool Active { get; set; } = true;
}
