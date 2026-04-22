using Anir.Data.Identity;
using Anir.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations;

public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.Property(u => u.FullName)
               .HasMaxLength(200)
               .IsRequired(false);

        // ELIMINADO: builder.Property(u => u.ImagenId).HasMaxLength(100).IsRequired(false);

        builder.Property(u => u.Active).HasDefaultValue(false);

        builder.Property(u => u.ThemeMode)
               .HasConversion<int>()
               .HasDefaultValue(ThemeMode.Auto);

        builder.Property(u => u.MustChangePassword).HasDefaultValue(true);

        builder.HasIndex(u => u.NormalizedUserName)
               .IsUnique()
               .HasDatabaseName("UX_Users_NormalizedUserName");

        builder.HasIndex(u => u.NormalizedEmail)
               .HasDatabaseName("IX_Users_NormalizedEmail");

        // RELACIÓN CON StoredFile
        builder.HasOne(u => u.ImageFile)
               .WithMany()
               .HasForeignKey(u => u.ImageFileId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}