using StackExchange.Redis;

namespace Bookshelf.API.RateLimiting
{
    public class RedisRateLimitStore : IRateLimitStore
    {
        // INCR + ustawienie czasu życia w jednym skrypcie, żeby klucz nie został bez wygasania,
        // gdyby proces padł między dwoma osobnymi poleceniami.
        private const string IncrementScript = """
            local count = redis.call('INCR', KEYS[1])
            if count == 1 then
                redis.call('PEXPIRE', KEYS[1], ARGV[1])
            end
            return count
            """;

        private readonly IConnectionMultiplexer _redis;

        public RedisRateLimitStore(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<long> IncrementAsync(string key, TimeSpan window)
        {
            var result = await _redis.GetDatabase().ScriptEvaluateAsync(
                IncrementScript,
                [new RedisKey(key)],
                [(long)window.TotalMilliseconds]);

            return (long)result;
        }
    }
}
