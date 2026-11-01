using Microsoft.Extensions.Caching.Memory;

namespace Bookshelf.API.RateLimiting
{
    /// <summary>
    /// Zamiennik bez Redisa (lokalny dev, docker-compose bez redis). Licznik żyje tylko w tej instancji.
    /// </summary>
    public class InMemoryRateLimitStore : IRateLimitStore
    {
        private readonly IMemoryCache _cache;
        private readonly object _lock = new();

        public InMemoryRateLimitStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<long> IncrementAsync(string key, TimeSpan window)
        {
            lock (_lock)
            {
                var counter = _cache.GetOrCreate(key, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = window;
                    return new Counter();
                })!;

                counter.Value++;
                return Task.FromResult(counter.Value);
            }
        }

        private sealed class Counter
        {
            public long Value;
        }
    }
}
