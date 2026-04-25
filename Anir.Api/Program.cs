using Anir.Api.Middlewares;
using Anir.Application.Common.Interfaces;
using Anir.Data;
using Anir.Data.Identity;
using Anir.Data.Seeders;
using Anir.Infrastructure.Jwt;
using Anir.Infrastructure.Reports;
using Anir.Infrastructure.Reports.Template.Excel;
using Anir.Infrastructure.Settings;
using Anir.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Serilog;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ============================================================
// LOGGING
// ============================================================
// Configuración de Serilog: consola + archivo diario, máximo 30 archivos
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Captura todo desde Debug hacia arriba
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",                 // Carpeta y nombre base
        rollingInterval: RollingInterval.Day,  // Un archivo por día
        retainedFileCountLimit: 30,            // Máximo 30 archivos
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Host.UseSerilog();

// ============================================================
// DATABASE
// ============================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ============================================================
// IDENTITY
// ============================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ============================================================
// JWT
// ============================================================
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // en dev puedes dejarlo false
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwt["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddAuthorization();


// Registrar implementación concreta de IFileStorage
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));

builder.Services.AddScoped<IFileStorageService, FileStorageService>();


// ============================================================
// EMAIL
// ============================================================
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);
builder.Services.AddScoped<IEmailService, EmailService>();


// ============================================================
// REPORTS
// ============================================================
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddScoped<IReportDataProvider, ReportDataProvider>();
builder.Services.AddScoped<OrganismReportExcel>();
builder.Services.AddScoped<CompanyReportExcel>();
builder.Services.AddScoped<UebReportExcel>();
builder.Services.AddScoped<PersonReportExcel>();
builder.Services.AddScoped<AnirWorkReportExce>();

// ============================================================
// CONTROLLERS + SWAGGER
// ============================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Anir API",
        Version = "v1",
        Description = "API de Anir con autenticación JWT"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,   // Http en lugar de ApiKey
        Scheme = "bearer",                // en minúsculas
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce tu token JWT en el campo: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================
// CORS
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            builder.Configuration["FrontendUrl"]!,
            builder.Configuration["BackendUrl"]!
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

// ============================================================
// BUILD APP
// ============================================================
var app = builder.Build();

// Routing (explícito)
app.UseRouting();

// Middleware global de excepciones lo más arriba posible
app.UseMiddleware<GlobalExceptionMiddleware>();

// ============================================================
// PERMITE QUE BLAZOR WASM MUESTRE LAS IMÁGENES DE LA API
// ============================================================
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var path = context.Request.Path.Value;
        if (path != null && path.StartsWith("/api/files/"))
        {
            context.Response.Headers.Append("Cross-Origin-Resource-Policy", "cross-origin");
        }
        return Task.CompletedTask;
    });
    await next();
});

// ============================================================
// STATIC FILES 
// ============================================================
app.UseStaticFiles();

// ============================================================
// MIDDLEWARE
// ============================================================
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");


app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// SWAGGER
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anir API v1");
        c.RoutePrefix = string.Empty; // Swagger en raíz: https://localhost:7253/
    });
}

// ============================================================
// SEEDERS
// ============================================================
await DatabaseSeeder.SeedAsync(app.Services);

// ============================================================
// MAP CONTROLLERS
// ============================================================
app.MapControllers();

app.Run();
