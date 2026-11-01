namespace Bookshelf.API.RateLimiting
{
    /// <summary>
    /// Licznik prób w oknie czasowym (fixed window).
    /// Azure: Redis — wspólny dla wszystkich instancji App Service. Lokalnie: pamięć procesu.
    /// </summary>
    public interface IRateLimitStore
    {
        /// <summary>Zwiększa licznik dla klucza i zwraca jego wartość w bieżącym oknie.</summary>
        Task<long> IncrementAsync(string key, TimeSpan window);
    }
}
