// Anir.Shared.Contracts.Companies/CompanyDto.cs
using System.ComponentModel.DataAnnotations;

namespace Anir.Shared.Contracts.Companies;

public class CompanyDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un organismo.")]
    [Display(Name = "Organismo")]
    public int OrganismId { get; set; }

    // Solo para mostrar en grilla/detalle
    public string? OrganismName { get; set; }


    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código no puede exceder {1} caracteres.")]
    [Display(Name = "Código DUINE")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "La abreviatura es obligatoria.")]
    [StringLength(50, ErrorMessage = "La abreviatura no puede exceder {1} caracteres.")]
    [Display(Name = "Abreviatura")]
    public string ShortName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder {1} caracteres.")]
    [Display(Name = "Nombre de la empresa")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La dirección no puede exceder {1} caracteres.")]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

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

