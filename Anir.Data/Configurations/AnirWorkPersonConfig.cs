using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations;

/// <summary>
/// Configuración EF Core para la entidad AnirWorkPerson.
/// Define restricciones, índices y relaciones.
/// </summary>
public class AnirWorkPersonConfig : IEntityTypeConfiguration<AnirWorkPerson>
{
    public void Configure(EntityTypeBuilder<AnirWorkPerson> builder)
    {
        builder.ToTable("AnirWorkPersons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ParticipationPercentage)
               .HasPrecision(5, 2);

        // Índice único para evitar duplicados por trabajo y persona
        builder.HasIndex(x => new { x.AnirWorkId, x.PersonId })
               .IsUnique();

        // Relaciones
        builder.HasOne(x => x.AnirWork)
               .WithMany(w => w.AnirWorkPersons)
               .HasForeignKey(x => x.AnirWorkId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Person)
               .WithMany(p => p.AnirWorkPersons)
               .HasForeignKey(x => x.PersonId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
