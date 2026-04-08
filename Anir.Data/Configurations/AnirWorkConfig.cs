using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations;

/// <summary>
/// EF Core configuration for the AnirWork entity.
/// Defines constraints, indexes, and relationships.
/// </summary>
public class AnirWorkConfig : IEntityTypeConfiguration<AnirWork>
{
    public void Configure(EntityTypeBuilder<AnirWork> builder)
    {
        builder.ToTable("AnirWorks");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.AnirNumber)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(a => a.Title)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(a => a.EconomicImpact)
               .HasColumnType("numeric(18,2)");

        // Boolean fields (no need for column type, EF Core maps them to BIT in SQL Server or boolean in PostgreSQL)
        builder.Property(a => a.HasSocialEffect)
               .IsRequired();

        builder.Property(a => a.HasEconomicEffect)
               .IsRequired();

        // Enum mapping
        builder.Property(a => a.Generalization)
               .HasConversion<int>() // store enum as int
               .IsRequired();

        builder.HasIndex(a => new { a.CompanyId, a.AnirNumber })
               .IsUnique();

        builder.HasIndex(a => new { a.CompanyId, a.ResolutionNumber })
               .IsUnique();

        builder.HasOne(a => a.Company)
               .WithMany(c => c.AnirWorks)
               .HasForeignKey(a => a.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
