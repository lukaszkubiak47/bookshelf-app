using Bookshelf.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookshelf.API.Controllers
{
    [ApiController]
    [Route("api/stats")]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _service;

        public StatsController(IStatsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Statystyki platformy na stronę główną.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetStats()
        {
            var result = await _service.GetStatsAsync();
            return Ok(result);
        }
    }
}