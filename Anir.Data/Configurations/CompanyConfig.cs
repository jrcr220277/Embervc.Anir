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

            builder.Property(c => c.Id).ValueGeneratedOnAdd(); 

            builder.Property(c => c.ShortName).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
            builder.Property(c => c.Address).HasMaxLength(250);
            builder.Property(c => c.Active).HasDefaultValue(true);

            builder.HasIndex(c => c.ShortName).IsUnique();
           
            builder.HasOne(c => c.Municipality)
                   .WithMany(m => m.Companies)
                   .HasForeignKey(c => c.MunicipalityId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.MunicipalityId);
        }
    }
}
