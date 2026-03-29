using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    /// <summary>
    /// Configuración EF Core para la entidad Person.
    /// Define restricciones, índices y relaciones con AnirWorkPerson.
    /// </summary>
    public class PersonConfig : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("Persons");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property(p => p.Dni).IsRequired().HasMaxLength(11);
            builder.Property(p => p.FullName).IsRequired().HasMaxLength(150);
            builder.Property(p => p.ImagenId).HasMaxLength(200);
            builder.Property(p => p.CellPhone).HasMaxLength(20);
            builder.Property(p => p.Email).HasMaxLength(100);

            builder.HasIndex(p => p.Dni).IsUnique();
        }
    }

}