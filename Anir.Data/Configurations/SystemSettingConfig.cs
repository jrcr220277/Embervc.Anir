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

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedOnAdd();

            builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
            builder.Property(s => s.LogoId).HasMaxLength(200);
            builder.Property(s => s.Address).HasMaxLength(300);
            builder.Property(s => s.Phone).HasMaxLength(50);
            builder.Property(s => s.Email).HasMaxLength(150);
            builder.Property(s => s.Website).HasMaxLength(150);
        }
    }

}
