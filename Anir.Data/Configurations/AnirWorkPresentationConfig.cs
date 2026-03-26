using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    /// <summary>
    /// Configuración EF Core para la entidad AnirWorkPresentation.
    /// Define restricciones y relaciones con AnirWork.
    /// </summary>
    public class AnirWorkPresentationConfig : IEntityTypeConfiguration<AnirWorkPresentation>
    {
        public void Configure(EntityTypeBuilder<AnirWorkPresentation> builder)
        {
            builder.ToTable("AnirWorkPresentations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Notes)
                   .HasMaxLength(500);

            builder.HasOne(x => x.AnirWork)
                   .WithMany(w => w.AnirWorkPresentations)
                   .HasForeignKey(x => x.AnirWorkId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
