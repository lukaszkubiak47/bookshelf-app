namespace Bookshelf.API.Models.DTOs.Stats
{
    /// <summary>
    /// Statystyki platformy wyświetlane m.in. na stronie głównej.
    /// </summary>
    public class StatsDto
    {
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCities { get; set; }
        public int ActiveOffers { get; set; }
    }
}