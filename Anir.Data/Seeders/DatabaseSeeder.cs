using Anir.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Anir.Data.Seeders
{
    /// <summary>
    /// Ejecuta todos los seeders del sistema ANIR.
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();


            await context.Database.MigrateAsync();

            if (!await roleManager.RoleExistsAsync("Administrador"))
            {
                await roleManager.CreateAsync(new IdentityRole("Administrador"));
            }

            await UserSeeder.SeedAsync(userManager, roleManager);
            await SystemSettingSeeder.SeedAsync(context);
            await ProvinceSeeder.SeedAsync(context);
            await MunicipalitySeeder.SeedAsync(context);
            await OrganismSeeder.SeedAsync(context);
            await CompanySeeder.SeedAsync(context);
            await UebSeeder.SeedAsync(context);
        }
    }

}
