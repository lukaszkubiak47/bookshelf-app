using System.Text.Json;
using Bookshelf.API.Exceptions;

namespace Bookshelf.API.Middleware
{
    /// <summary>
    /// Globalny middleware łapiący wszystkie wyjątki i zwracający ustandaryzowane odpowiedzi JSON.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BookshelfException ex)
            {
                // Nasze własne wyjątki: mają zdefiniowany StatusCode
                _logger.LogWarning("Bookshelf exception: {Message}", ex.Message);
                await WriteErrorResponse(context, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                // Nieoczekiwane błędy: logujemy szczegóły, userowi pokazujemy ogólny komunikat
                _logger.LogError(ex, "Nieoczekiwany błąd serwera");
                await WriteErrorResponse(context, 500, "Wewnętrzny błąd serwera");
            }
        }

        private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode,
                message
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}