using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations;

/// <summary>
/// Configuración EF Core para la entidad AnirWork.
/// Define restricciones, índices y relaciones.
/// </summary>
public class AnirWorkConfig : IEntityTypeConfiguration<AnirWork>
{
    public void Configure(EntityTypeBuilder<AnirWork> builder)
    {
        builder.ToTable("AnirWorks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AnirNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Recommendations)
            .HasMaxLength(300);

        builder.Property(x => x.ResolutionNumber)
            .HasMaxLength(50);

        builder.Property(x => x.EconomicImpact)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ImageId)
            .HasMaxLength(100);

        builder.Property(x => x.PdfId)
            .HasMaxLength(100);

        // Índices únicos recomendados
        builder.HasIndex(x => new { x.CompanyId, x.AnirNumber })
               .IsUnique();

        builder.HasIndex(x => new { x.CompanyId, x.ResolutionNumber })
               .IsUnique();

        // Relaciones
        builder.HasOne(x => x.Company)
            .WithMany(c => c.AnirWorks)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AnirWorkPersons)
            .WithOne(p => p.AnirWork)
            .HasForeignKey(p => p.AnirWorkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AnirWorkPresentations)
            .WithOne(p => p.AnirWork)
            .HasForeignKey(p => p.AnirWorkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
