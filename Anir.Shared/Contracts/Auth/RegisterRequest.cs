using System.ComponentModel.DataAnnotations;

namespace Anir.Shared.Contracts.Auth;

public class RegisterRequest
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

    [Required(ErrorMessage = "Confirme la nueva contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nueva contraseña")]
    [Compare(nameof(Password), ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [Display(Name = "Nombre completo")]
    public string? FullName { get; set; }
   
    [Display(Name = "Imagen perfil")]
    public string? ImagenId { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; } = false;

    [Required]
    [Display(Name = "Roles")]
    public List<string> Roles { get; set; } = new();
}
