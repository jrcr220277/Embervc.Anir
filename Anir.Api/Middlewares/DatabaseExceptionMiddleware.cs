using Anir.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Anir.Api.Middlewares
{
    /// <summary>
    /// Middleware global para capturar excepciones de base de datos.
    /// Usa DatabaseErrorHelper para clasificar el error y devolver
    /// respuestas uniformes sin ensuciar los controladores.
    /// </summary>
    public class DatabaseExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DatabaseExceptionMiddleware> _logger;

        public DatabaseExceptionMiddleware(RequestDelegate next, ILogger<DatabaseExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var classification = DatabaseErrorHelper.Classify(ex);

                _logger.LogError(ex,
                    "Error inesperado. Clasificación: {Classification}, Ruta: {Path}",
                    classification,
                    context.Request.Path);

                context.Response.ContentType = "application/json";

                // Caso especial: concurrencia EF Core
                if (ex is DbUpdateConcurrencyException)
                    classification = "Concurrency";

                switch (classification)
                {
                    // -------------------------
                    // Conflictos de unicidad
                    // -------------------------
                    case "UniqueViolation":
                    case "DuplicateEntry":
                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("Ya existe un registro con esos datos.")
                        );
                        break;

                    // -------------------------
                    // Restricciones FK
                    // -------------------------
                    case "ForeignKeyViolation":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("El registro está relacionado y no puede eliminarse.")
                        );
                        break;

                    // -------------------------
                    // Campos obligatorios
                    // -------------------------
                    case "NotNullViolation":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("Faltan datos obligatorios.")
                        );
                        break;

                    // -------------------------
                    // Deadlocks
                    // -------------------------
                    case "Deadlock":
                        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("La operación no pudo completarse por un bloqueo.")
                        );
                        break;

                    // -------------------------
                    // Timeout
                    // -------------------------
                    case "Timeout":
                        context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("La operación excedió el tiempo de espera.")
                        );
                        break;

                    // -------------------------
                    // Concurrencia EF Core
                    // -------------------------
                    case "Concurrency":
                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("Otro usuario modificó este registro.")
                        );
                        break;

                    // -------------------------
                    // Longitudes y rangos
                    // -------------------------
                    case "StringTooLong":
                    case "DataTooLong":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("El valor es demasiado largo para la columna.")
                        );
                        break;

                    case "NumericOutOfRange":
                    case "OutOfRangeValue":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("El valor numérico está fuera de rango.")
                        );
                        break;

                    // -------------------------
                    // Conversiones
                    // -------------------------
                    case "ArithmeticOverflow":
                    case "ConversionError":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("Error de conversión o desbordamiento numérico.")
                        );
                        break;

                    case "InvalidDateTime":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("Formato de fecha/hora inválido.")
                        );
                        break;

                    // -------------------------
                    // Errores internos PostgreSQL
                    // -------------------------
                    case "UndefinedFunction":
                    case "DatatypeMismatch":
                    case "InvalidEncoding":
                    case "InternalDatabaseError":
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("Error interno en la base de datos.")
                        );
                        break;

                    // -------------------------
                    // Desconocido
                    // -------------------------
                    default:
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(
                            ProcessResponse<string>.Fail("Error inesperado en el servidor.")
                        );
                        break;
                }
            }
        }
    }
}
