using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    public class OrganismConfig : IEntityTypeConfiguration<Organism>
    {
        public void Configure(EntityTypeBuilder<Organism> builder)
        {
            builder.ToTable("Organisms");

            builder.HasKey(o => o.Id);
            builder.Property(p => p.Id).UseIdentityByDefaultColumn();

            // Código DUINE: 00113, 00102, etc.
            builder.Property(o => o.Code)
                   .IsRequired()
                   .HasMaxLength(10);

            // Sigla: MINAL, MINCEX, MINSAP…
            builder.Property(o => o.ShortName)
                   .IsRequired()
                   .HasMaxLength(20);

            // Nombre completo del organismo
            builder.Property(o => o.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            // Índices
            builder.HasIndex(o => o.Code).IsUnique();
            builder.HasIndex(o => o.ShortName).IsUnique();
            builder.HasIndex(o => o.Name).IsUnique();

            // Relación con Company (1 Organism → N Companies)
            builder.HasMany(o => o.Companies)
                   .WithOne(c => c.Organism)
                   .HasForeignKey(c => c.OrganismId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
