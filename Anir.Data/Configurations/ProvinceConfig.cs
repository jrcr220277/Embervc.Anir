using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    /// <summary>
    /// Configuración EF Core para la entidad Province.
    /// Define índices únicos y relaciones con Municipality.
    /// </summary>
    public class ProvinceConfig : IEntityTypeConfiguration<Province>
    {
        public void Configure(EntityTypeBuilder<Province> builder)
        {
            builder.ToTable("Provinces");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property(p => p.ShortName).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(150);

            builder.HasIndex(p => p.Name).IsUnique();
            builder.HasIndex(p => p.ShortName).IsUnique();
        }
    }

}
