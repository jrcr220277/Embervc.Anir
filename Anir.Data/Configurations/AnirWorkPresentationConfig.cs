using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations;

public class AnirWorkPresentationConfig : IEntityTypeConfiguration<AnirWorkPresentation>
{
    public void Configure(EntityTypeBuilder<AnirWorkPresentation> builder)
    {
        builder.ToTable("AnirWorkPresentations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Notes)
               .HasMaxLength(500);

        // ✔️ Relación con AnirWork (padre)
        builder.HasOne(x => x.AnirWork)
               .WithMany(a => a.AnirWorkPresentations)
               .HasForeignKey(x => x.AnirWorkId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
