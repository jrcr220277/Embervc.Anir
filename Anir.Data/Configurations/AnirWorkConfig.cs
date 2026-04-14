using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AnirWorkConfig : IEntityTypeConfiguration<AnirWork>
{
    public void Configure(EntityTypeBuilder<AnirWork> builder)
    {
        builder.ToTable("AnirWorks");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        // ============================
        // CAMPOS BASE
        // ============================
        builder.Property(a => a.AnirNumber)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(a => a.Title)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(a => a.Description)
               .HasMaxLength(2000);

        builder.Property(a => a.EconomicImpact)
               .HasColumnType("numeric(18,2)");

        builder.Property(a => a.HasSocialEffect).IsRequired();
        builder.Property(a => a.HasEconomicEffect).IsRequired();

        builder.Property(a => a.Generalization)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(a => a.Recommendations)
               .HasMaxLength(2000);

        builder.Property(a => a.ResolutionNumber)
               .HasMaxLength(50);

        // ============================
        // ÍNDICES PROFESIONALES
        // ============================
        builder.HasIndex(a => new { a.UebId, a.AnirNumber }).IsUnique();
        builder.HasIndex(a => new { a.UebId, a.ResolutionNumber }).IsUnique();

        // ============================
        // RELACIÓN CORRECTA (UEB)
        // ============================
        builder.HasOne(a => a.Ueb)
               .WithMany(u => u.AnirWorks)
               .HasForeignKey(a => a.UebId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
