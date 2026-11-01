using Bookshelf.API.Data;
using Bookshelf.API.Models.DTOs.Stats;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf.API.Services
{
    public class StatsService : IStatsService
    {
        private const string AvailableStatus = "available";
        private const string CacheKey = "stats";

        // Statystyki strony głównej mogą być lekko nieaktualne — oszczędzamy 4 zapytania do bazy przy każdym wejściu.
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private readonly AppDbContext _db;
        private readonly ICacheService _cache;

        public StatsService(AppDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public Task<StatsDto> GetStatsAsync()
        {
            return _cache.GetOrCreateAsync(CacheKey, CountStatsAsync, CacheDuration);
        }

        private async Task<StatsDto> CountStatsAsync()
        {
            // Każda statystyka to osobne zapytanie zliczające, bez ładowania encji.
            return new StatsDto
            {
                TotalBooks = await _db.Books.CountAsync(),
                TotalUsers = await _db.Users.CountAsync(),
                TotalCities = await _db.Users
                    .Select(u => u.City)
                    .Distinct()
                    .CountAsync(),
                ActiveOffers = await _db.UserBooks
                    .CountAsync(ub =>
                        ub.Status == AvailableStatus &&
                        !ub.TransactionRequests.Any(tr =>
                            tr.Status == "accepted" || tr.Status == "done"))
            };
        }
    }
}