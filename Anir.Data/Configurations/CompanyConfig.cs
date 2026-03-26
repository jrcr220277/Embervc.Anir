using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Anir.Data.Configurations
{
    /// <summary>
    /// Configuración EF Core para la entidad Company.
    /// Define índices, restricciones y relaciones con Municipality.
    /// </summary>
    public class CompanyConfig : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");

            builder.HasKey(c => c.Id);

            // Índices únicos
            builder.HasIndex(c => c.ShortName)
                   .IsUnique();

            builder.HasIndex(c => c.Name)
                   .IsUnique();

            // Relaciones
            builder.HasOne(c => c.Municipality)
                   .WithMany(m => m.Companies)
                   .HasForeignKey(c => c.MunicipalityId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
