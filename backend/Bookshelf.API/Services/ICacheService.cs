namespace Bookshelf.API.Services
{
    /// <summary>
    /// Cache rozproszony (Azure Managed Redis; lokalnie bez Redisa — pamięć procesu).
    /// Błąd cache nigdy nie psuje żądania: traktujemy go jak brak trafienia.
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T value, TimeSpan ttl);

        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl) where T : class;
    }
}
