using Anir.Data.Identity;
using Anir.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations;

/// <summary>
/// Configuración EF Core para la entidad ApplicationUser.
/// Define longitudes, índices y propiedades adicionales.
/// </summary>
public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Mantener el nombre por defecto de Identity
        builder.ToTable("AspNetUsers");

        builder.Property(u => u.FullName)
               .HasMaxLength(200)
               .IsRequired(false);

        builder.Property(u => u.ImagenId)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(u => u.Active)
               .HasDefaultValue(false);

        builder.Property(u => u.ThemeMode)
               .HasConversion<int>() 
               .HasDefaultValue(ThemeMode.Auto);

        builder.Property(u => u.MustChangePassword)
               .HasDefaultValue(true);

        // Índices importantes
        builder.HasIndex(u => u.NormalizedUserName)
               .IsUnique()
               .HasDatabaseName("UX_Users_NormalizedUserName");

        builder.HasIndex(u => u.NormalizedEmail)
               .HasDatabaseName("IX_Users_NormalizedEmail");
    }
}
