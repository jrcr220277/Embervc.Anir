using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Anir.Shared.Contracts.Common; // ProcessResponse<T>
using Anir.Shared.Helpers; // DatabaseErrorHelper

namespace Anir.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                // Logueo centralizado (Serilog configurado en Program.cs recogerá esto)
                _logger.LogError(ex, "Error inesperado. Ruta: {Path}", context.Request.Path);

                context.Response.ContentType = "application/json";

                // Si ya se empezó a escribir el body (ej: error durante FileStream), 
                // no intentamos escribir JSON porque fallaría
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "Error después de iniciar la respuesta. No se puede escribir JSON de error.");
                    throw; // Deja que el host maneje la desconexión limpiamente
                }
                // Si es excepción de BD, mantenemos tu clasificación actual
                var classification = DatabaseErrorHelper.Classify(ex);
                if (ex is DbUpdateConcurrencyException)
                    classification = "Concurrency";

                // Manejo específico para errores de archivos / IO
                if (ex is FileNotFoundException)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Archivo no encontrado."));
                    return;
                }

                if (ex is UnauthorizedAccessException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Ruta inválida o acceso denegado."));
                    return;
                }

                if (ex is IOException)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Error de entrada/salida en el servidor."));
                    return;
                }

                // Si la clasificación indica error de BD, aplicamos tu switch original
                switch (classification)
                {
                    case "UniqueViolation":
                    case "DuplicateEntry":
                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Ya existe un registro con esos datos."));
                        return;

                    case "ForeignKeyViolation":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("El registro está relacionado y no puede eliminarse."));
                        return;

                    case "NotNullViolation":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Faltan datos obligatorios."));
                        return;

                    case "Deadlock":
                        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("La operación no pudo completarse por un bloqueo."));
                        return;

                    case "Timeout":
                        context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("La operación excedió el tiempo de espera."));
                        return;

                    case "Concurrency":
                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Otro usuario modificó este registro."));
                        return;

                    case "StringTooLong":
                    case "DataTooLong":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("El valor es demasiado largo para la columna."));
                        return;

                    case "NumericOutOfRange":
                    case "OutOfRangeValue":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("El valor numérico está fuera de rango."));
                        return;

                    case "ArithmeticOverflow":
                    case "ConversionError":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Error de conversión o desbordamiento numérico."));
                        return;

                    case "InvalidDateTime":
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Formato de fecha/hora inválido."));
                        return;

                    case "UndefinedFunction":
                    case "DatatypeMismatch":
                    case "InvalidEncoding":
                    case "InternalDatabaseError":
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Error interno en la base de datos."));
                        return;

                    default:
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(ProcessResponse<string>.Fail("Error inesperado en el servidor."));
                        return;
                }
            }
        }
    }
}
