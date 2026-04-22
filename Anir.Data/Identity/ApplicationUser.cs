using Anir.Data.Entities;
using Anir.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace Anir.Data.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // ANTES: public string? ImagenId { get; set; }
        public int? ImageFileId { get; set; }
        public StoredFile? ImageFile { get; set; }

        public string? FullName { get; set; }
        public bool Active { get; set; } = false;
        public bool MustChangePassword { get; set; } = true;
        public ThemeMode ThemeMode { get; set; } = ThemeMode.Auto;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public bool ProfileCompleted { get; set; } = false;
    }
}