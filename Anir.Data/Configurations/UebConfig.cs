using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    public class UebConfig : IEntityTypeConfiguration<Ueb>
    {
        public void Configure(EntityTypeBuilder<Ueb> builder)
        {
            builder.ToTable("Uebs");

            builder.HasKey(u => u.Id);
            builder.Property(p => p.Id).UseIdentityByDefaultColumn();

            // Código DUINE
            builder.Property(u => u.Code)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.HasIndex(u => u.Code).IsUnique();

            // Nombre
            builder.Property(u => u.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(u => u.Address).HasMaxLength(250);
            builder.Property(u => u.Phone).HasMaxLength(50);
            builder.Property(u => u.Email).HasMaxLength(150);

            builder.Property(u => u.Active)
                   .HasDefaultValue(true);

            builder.HasIndex(u => u.MunicipalityId);
            builder.HasIndex(u => u.CompanyId);

            // Relación con Municipality
            builder.HasOne(u => u.Municipality)
                   .WithMany(m => m.Uebs)
                   .HasForeignKey(u => u.MunicipalityId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Relación con Company
            builder.HasOne(u => u.Company)
                   .WithMany(c => c.Uebs)
                   .HasForeignKey(u => u.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
