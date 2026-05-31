using Bookshelf.API.Models.DTOs.Stats;

namespace Bookshelf.API.Services
{
    public interface IStatsService
    {
        // Zwraca zagregowane statystyki platformy.
        Task<StatsDto> GetStatsAsync();
    }
}