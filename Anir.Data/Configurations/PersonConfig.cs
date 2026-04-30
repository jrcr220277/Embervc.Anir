using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    public class PersonConfig : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("Persons");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property(p => p.Dni).IsRequired().HasMaxLength(11);
            builder.Property(p => p.FullName).IsRequired().HasMaxLength(150);
            // ELIMINADO: builder.Property(p => p.ImagenId).HasMaxLength(200);
            builder.Property(p => p.CellPhone).HasMaxLength(20);
            builder.Property(p => p.Email).HasMaxLength(100);

            builder.HasIndex(p => p.Dni).IsUnique();

            // ═══════════════════════════════════════════════════════════
            // NUEVOS CAMPOS ENUM - EF Core los mapea como int por defecto
            // Si necesitas almacenar como string, usar: .HasConversion<string>()
            // ═══════════════════════════════════════════════════════════
            builder.Property(p => p.Affiliation).HasConversion<int>();
            builder.Property(p => p.SchoolLevel).HasConversion<int>();
            builder.Property(p => p.Sex).HasConversion<int>();
            builder.Property(p => p.ExecutiveRole).HasConversion<int>();
            builder.Property(p => p.Militancy).HasConversion<int>();
            // ═══════════════════════════════════════════════════════════

            // RELACIÓN CON StoredFile
            builder.HasOne(p => p.ImageFile)
                   .WithMany()
                   .HasForeignKey(p => p.ImageFileId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}