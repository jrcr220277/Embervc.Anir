using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Anir.Shared.Contracts.Auth
{
    /// <summary>
    /// Request para cambiar la contraseña del usuario autenticado.
    /// Validaciones por DataAnnotations pensadas para buena UX.
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        [PasswordPropertyText]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos {1} caracteres.")]
        [MaxLength(256, ErrorMessage = "La nueva contraseña no puede exceder {1} caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare(nameof(NewPassword), ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
