using System.ComponentModel.DataAnnotations;

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre {2} y {1} caracteres.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación es obligatoria.")]
    [Compare(nameof(NewPassword), ErrorMessage = "La confirmación no coincide.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
