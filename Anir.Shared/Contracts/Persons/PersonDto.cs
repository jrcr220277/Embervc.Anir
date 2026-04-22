using System.ComponentModel.DataAnnotations;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Enums;

namespace Anir.Shared.Contracts.Persons;

public class PersonDto
{
    public int Id { get; set; }

    [Display(Name = "Imagen")]
    public FileResponse? ImageFile { get; set; }

    [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
    [StringLength(11, ErrorMessage = "El documento no puede exceder {1} caracteres.")]
    [Display(Name = "Documento de identidad")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder {1} caracteres.")]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder {1} caracteres.")]
    [Display(Name = "Teléfono móvil")]
    public string? CellPhone { get; set; }

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder {1} caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string? Email { get; set; }

    [Display(Name = "Afiliacion")]
    public PersonAffiliation Affiliation { get; set; }

    [StringLength(250, ErrorMessage = "La descripción no puede exceder {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; } = true;

    [Display(Name = "Trabajos ANIR")]
    public int? AnirWorkCount { get; set; }
}