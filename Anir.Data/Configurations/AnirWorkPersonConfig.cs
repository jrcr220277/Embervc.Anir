using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations;

public class AnirWorkPersonConfig : IEntityTypeConfiguration<AnirWorkPerson>
{
    public void Configure(EntityTypeBuilder<AnirWorkPerson> builder)
    {
        builder.ToTable("AnirWorkPersons");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.ParticipationPercentage)
               .HasPrecision(5, 2);

        builder.HasIndex(x => new { x.AnirWorkId, x.PersonId }).IsUnique();

        // ✔️ Relación con AnirWork (padre)
        builder.HasOne(x => x.AnirWork)
               .WithMany(a => a.AnirWorkPersons)
               .HasForeignKey(x => x.AnirWorkId)
               .OnDelete(DeleteBehavior.Cascade);

        // ✔️ Relación con Person (padre)
        builder.HasOne(x => x.Person)
               .WithMany(p => p.AnirWorkPersons)
               .HasForeignKey(x => x.PersonId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
