using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    /// <summary>
    /// Configuración EF Core para la entidad Municipality.
    /// Define índices únicos y relaciones con Province y Company.
    /// </summary>
    public class MunicipalityConfig : IEntityTypeConfiguration<Municipality>
    {
        public void Configure(EntityTypeBuilder<Municipality> builder)
        {
            builder.ToTable("Municipalities");

            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedOnAdd();

            builder.Property(m => m.Name).IsRequired().HasMaxLength(150);
            builder.Property(m => m.ProvinceId).IsRequired();

            builder.HasIndex(m => new { m.ProvinceId, m.Name }).IsUnique();

            builder.HasOne(m => m.Province)
                   .WithMany(p => p.Municipalities)
                   .HasForeignKey(m => m.ProvinceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
