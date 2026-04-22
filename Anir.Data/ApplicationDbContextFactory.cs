using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Anir.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // COPIA EXACTAMENTE TU CADENA DE CONEXIÓN DE appsettings.json AQUÍ
        optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=AnirDB;User Id=postgres;Password=123");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}