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

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.AnirNumber).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(150);
        builder.Property(a => a.EconomicImpact).HasColumnType("numeric(18,2)");

        builder.HasIndex(a => new { a.CompanyId, a.AnirNumber }).IsUnique();
        builder.HasIndex(a => new { a.CompanyId, a.ResolutionNumber }).IsUnique();

        builder.HasOne(a => a.Company)
               .WithMany(c => c.AnirWorks)
               .HasForeignKey(a => a.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

