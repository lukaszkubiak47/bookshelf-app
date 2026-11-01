using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Bookshelf.API.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var json = await _cache.GetStringAsync(key);
                return json == null ? default : JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                // Redis niedostępny albo uszkodzony wpis -> zachowujemy się jak przy braku trafienia.
                _logger.LogWarning(ex, "Odczyt z cache nie powiódł się (klucz {Key})", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Zapis do cache nie powiódł się (klucz {Key})", key);
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl) where T : class
        {
            var cached = await GetAsync<T>(key);
            if (cached != null)
                return cached;

            var value = await factory();
            await SetAsync(key, value, ttl);
            return value;
        }
    }
}
