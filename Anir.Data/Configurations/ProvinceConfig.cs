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

            // Índices únicos
            builder.HasIndex(p => p.ShortName)
                   .IsUnique();

            builder.HasIndex(p => p.Name)
                   .IsUnique();

            // Relación con Municipalities
            builder.HasMany(p => p.Municipalities)
                   .WithOne(m => m.Province)
                   .HasForeignKey(m => m.ProvinceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
