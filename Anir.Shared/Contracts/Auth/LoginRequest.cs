using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Anir.Shared.Contracts.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre {2} y {1} caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string Password { get; set; } = string.Empty;
}
