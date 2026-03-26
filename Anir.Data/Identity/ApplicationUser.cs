using Anir.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using System;

namespace Anir.Data.Identity
{
    /// <summary>
    /// Usuario extendido para ANIR.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>Id de la imagen en FileStorage.</summary>
        public string? ImagenId { get; set; }

        /// <summary>Nombre completo.</summary>
        public string? FullName { get; set; }

        /// <summary>Cuenta activa para iniciar sesión.</summary>
        public bool Active { get; set; } = false;

        /// <summary>Forzar cambio de contraseña en próximo login.</summary>
        public bool MustChangePassword { get; set; } = true;

        /// <summary>Preferencia de tema (Auto, Light, Dark).</summary>
        public ThemeMode ThemeMode { get; set; } = ThemeMode.Auto;

        /// <summary>Fecha de creación del usuario.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Última vez que el usuario actualizó su perfil.</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Último inicio de sesión exitoso.</summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>IP del último inicio de sesión (opcional).</summary>
        public string? LastLoginIp { get; set; }

        /// <summary>Indica si el perfil está completo (opcional).</summary>
        public bool ProfileCompleted { get; set; } = false;
    }
}
