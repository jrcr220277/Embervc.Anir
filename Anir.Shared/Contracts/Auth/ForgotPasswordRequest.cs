using System.ComponentModel.DataAnnotations;

namespace Anir.Shared.Contracts.Auth
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;
    }
}
