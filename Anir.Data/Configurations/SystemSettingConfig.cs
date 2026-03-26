using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    /// <summary>
    /// Configuración EF Core para la entidad SystemSetting.
    /// Define restricciones y longitudes de propiedades.
    /// </summary>
    public class SystemSettingConfig : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.ToTable("SystemSettings");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.Email)
                   .HasMaxLength(150);

            builder.Property(e => e.Phone)
                   .HasMaxLength(50);

            builder.Property(e => e.Website)
                   .HasMaxLength(150);

            builder.Property(e => e.Address)
                   .HasMaxLength(300);

            builder.Property(e => e.LogoId)
                   .HasMaxLength(200);
        }
    }
}
