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
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Contraseña nueva")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        [Display(Name = "Confirmar nueva contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
