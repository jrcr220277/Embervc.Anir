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

        builder.Property(a => a.AnirNumber)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(a => a.Title)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(a => a.Description).HasMaxLength(2000);

        builder.Property(a => a.EconomicImpact)
               .HasColumnType("numeric(18,2)");

        builder.Property(a => a.HasSocialEffect).IsRequired();
        builder.Property(a => a.HasEconomicEffect).IsRequired();

        builder.Property(a => a.Generalization)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(a => a.Recommendations).HasMaxLength(2000);
        builder.Property(a => a.ResolutionNumber).HasMaxLength(50);

        builder.HasIndex(a => new { a.CompanyId, a.AnirNumber }).IsUnique();
        builder.HasIndex(a => new { a.CompanyId, a.ResolutionNumber }).IsUnique();

        // ✔️ Relación con Company (HIJO)
        builder.HasOne(a => a.Company)
               .WithMany(c => c.AnirWorks)
               .HasForeignKey(a => a.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
