using Bookshelf.API.Data;
using Bookshelf.API.Models.DTOs.Stats;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf.API.Services
{
    public class StatsService : IStatsService
    {
        private const string AvailableStatus = "available";

        private readonly AppDbContext _db;

        public StatsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<StatsDto> GetStatsAsync()
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