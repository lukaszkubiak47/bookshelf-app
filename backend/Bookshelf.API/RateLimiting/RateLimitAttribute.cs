using System.Security.Claims;
using Bookshelf.API.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bookshelf.API.RateLimiting
{
    /// <summary>
    /// Limit prób dla akcji kontrolera, np. [RateLimit("login", 10, 300)] = 10 prób na 5 minut.
    /// Licznik per zalogowany użytkownik (jeśli jest), w przeciwnym razie per adres IP.
    /// Po przekroczeniu: 429 przez ExceptionHandlingMiddleware.
    /// Wyłączenie (np. na czas testu obciążeniowego): RateLimiting:Enabled = false.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _policy;
        private readonly int _limit;
        private readonly TimeSpan _window;

        public RateLimitAttribute(string policy, int limit, int windowSeconds)
        {
            _policy = policy;
            _limit = limit;
            _window = TimeSpan.FromSeconds(windowSeconds);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var services = context.HttpContext.RequestServices;
            var configuration = services.GetRequiredService<IConfiguration>();

            if (!configuration.GetValue("RateLimiting:Enabled", true))
            {
                await next();
                return;
            }

            var key = $"ratelimit:{_policy}:{GetPartition(context.HttpContext)}";
            long count;

            try
            {
                count = await services.GetRequiredService<IRateLimitStore>().IncrementAsync(key, _window);
            }
            catch (Exception ex)
            {
                // Redis niedostępny -> nie blokujemy użytkowników (dostępność ważniejsza niż limit).
                services.GetRequiredService<ILogger<RateLimitAttribute>>()
                    .LogWarning(ex, "Licznik limitu prób niedostępny (polityka {Policy})", _policy);
                await next();
                return;
            }

            if (count > _limit)
            {
                context.HttpContext.Response.Headers.RetryAfter = ((int)_window.TotalSeconds).ToString();
                throw new TooManyRequestsException("Zbyt wiele prób. Spróbuj ponownie za kilka minut.");
            }

            await next();
        }

        private static string GetPartition(HttpContext httpContext)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                return $"user:{userId}";

            // Za App Service prawdziwy adres klienta ustawia UseForwardedHeaders (Program.cs).
            return $"ip:{httpContext.Connection.RemoteIpAddress}";
        }
    }
}
