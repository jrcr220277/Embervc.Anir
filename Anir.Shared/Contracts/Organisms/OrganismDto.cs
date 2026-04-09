using System.ComponentModel.DataAnnotations;

namespace Anir.Shared.Contracts.Organisms;

public class OrganismDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(10)]
    [Display(Name = "Codigo")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "La sigla es obligatoria.")]
    [StringLength(20)]
    [Display(Name = "Abreviatura")]
    public string ShortName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del organismo es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;
}
