using Anir.Client;
using Anir.Client.Services;
using Anir.Client.Services.Alerts;
using Anir.Client.Services.AnirWorks;
using Anir.Client.Services.Auth;
using Anir.Client.Services.Company;

using Anir.Client.Services.Organism;
using Anir.Client.Services.Person;
using Anir.Client.Services.SystemSettings;
using Anir.Client.Services.Ueb;
using Anir.Web.Client.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Leer configuración según ambiente, appsetting
var backendUrl = builder.Configuration["BackendUrl"];
var frontendUrl = builder.Configuration["FrontendUrl"];

// ------------------------------------------------------------
// 1. MudBlazor
// ------------------------------------------------------------
builder.Services.AddMudServices();

// ------------------------------------------------------------
// 2. Alertas
// ------------------------------------------------------------
builder.Services.AddScoped<IAlertService, MudAlertService>();

// ------------------------------------------------------------
// 3. HttpClient para la API (sin hardcodear URL)
// ------------------------------------------------------------
// HttpClient configurado dinámicamente
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(backendUrl!)
});


// ------------------------------------------------------------
// 4. LocalStorage (para guardar el JWT)
// ------------------------------------------------------------
builder.Services.AddBlazoredLocalStorage();

// ------------------------------------------------------------
// 5. Autenticación con JWT
// ------------------------------------------------------------
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationProviderJWT>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthenticationProviderJWT>());

// ------------------------------------------------------------
// 6. UserState (estado PRO del usuario)
// ------------------------------------------------------------
builder.Services.AddScoped<UserState>();

// ------------------------------------------------------------
// 7. Servicios del Cliente
// ------------------------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<ProvinceService>();
builder.Services.AddScoped<MunicipalityService>();
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IOrganismService, OrganismService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IUebService, UebService>();
builder.Services.AddScoped<IAnirWorkService, AnirWorkService>();

// ------------------------------------------------------------
// 8. Construir la app
// ------------------------------------------------------------
var host = builder.Build();

// ------------------------------------------------------------
// 9. Reconstrucción automática del usuario al iniciar
// ------------------------------------------------------------
var authProvider = host.Services.GetRequiredService<AuthenticationProviderJWT>();
var authService = host.Services.GetRequiredService<IAuthService>();
var userState = host.Services.GetRequiredService<UserState>();

// ------------------------------------------------------------
// 10. Ejecutar la aplicación
// ------------------------------------------------------------
await host.RunAsync();
