using Anir.Data.Entities;
using Anir.Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Anir.Data
{
    /// <summary>
    /// DbContext principal del sistema ANIR.
    /// Incluye las entidades del dominio y la integración con Identity.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets del dominio
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<StoredFile> StoredFiles { get; set; }
        public DbSet<Organism> Organisms { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Ueb> Uebs { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Municipality> Municipalities { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<AnirWork> AnirWorks { get; set; }
        public DbSet<AnirWorkPerson> AnirWorkPersons { get; set; }
        public DbSet<AnirWorkPresentation> AnirWorkPresentations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversión del enum ThemeMode a string
            modelBuilder.Entity<ApplicationUser>().Property(u => u.ThemeMode).HasConversion<string>();

            // Aplica todas las configuraciones EF desde Infrastructure
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Extensión de PostgreSQL (si la usas)
            modelBuilder.HasPostgresExtension("uuid-ossp");

        }
    }
}
