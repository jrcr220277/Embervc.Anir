using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    public class CompanyConfig : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");

            builder.HasKey(c => c.Id);
            builder.Property(p => p.Id).UseIdentityByDefaultColumn();

            // Código DUINE
            builder.Property(c => c.Code)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.HasIndex(c => c.Code).IsUnique();

            // Campos básicos
            builder.Property(c => c.ShortName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(c => c.Address).HasMaxLength(250);
            builder.Property(c => c.Phone).HasMaxLength(50);
            builder.Property(c => c.Email).HasMaxLength(150);

            builder.Property(c => c.Active)
                   .HasDefaultValue(true);

            builder.HasIndex(c => c.ShortName).IsUnique();
            builder.HasIndex(c => c.MunicipalityId);
            builder.HasIndex(c => c.OrganismId);

            // Relación con Municipality
            builder.HasOne(c => c.Municipality)
                   .WithMany(m => m.Companies)
                   .HasForeignKey(c => c.MunicipalityId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Relación con Organism
            builder.HasOne(c => c.Organism)
                   .WithMany(o => o.Companies)
                   .HasForeignKey(c => c.OrganismId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
