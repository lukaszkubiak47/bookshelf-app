using Bookshelf.API.Models.DTOs.Transactions;
using Bookshelf.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookshelf.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _service;

        public TransactionsController(ITransactionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Wyslij prosbe o ksiazke.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
        {
            // Pobierz ID requestera z JWT
            var userId = GetCurrentUserId();

            // Serwis waliduje oferte, status i duplikaty pending
            var result = await _service.CreateAsync(userId, dto);

            return CreatedAtAction(nameof(GetSent), null, result);
        }

        /// <summary>
        /// Pobierz prosby otrzymane dla moich ksiazek.
        /// </summary>
        [HttpGet("received")]
        public async Task<IActionResult> GetReceived()
        {
            // Aktualny user jest wlascicielem ofert
            var userId = GetCurrentUserId();
            var result = await _service.GetReceivedAsync(userId);

            return Ok(result);
        }

        /// <summary>
        /// Pobierz prosby wyslane przeze mnie.
        /// </summary>
        [HttpGet("sent")]
        public async Task<IActionResult> GetSent()
        {
            // Aktualny user jest requesterem
            var userId = GetCurrentUserId();
            var result = await _service.GetSentAsync(userId);

            return Ok(result);
        }

        /// <summary>
        /// Zaakceptuj prosbe o moja ksiazke.
        /// </summary>
        [HttpPut("{id:guid}/accept")]
        public async Task<IActionResult> Accept(Guid id, [FromBody] AcceptTransactionDto dto)
        {
            // Tylko wlasciciel oferty moze akceptowac prosbe
            var userId = GetCurrentUserId();
            var result = await _service.AcceptAsync(userId, id, dto);

            return Ok(result);
        }

        /// <summary>
        /// Odrzuc prosbe o moja ksiazke.
        /// </summary>
        [HttpPut("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            // Tylko wlasciciel oferty moze odrzucic prosbe
            var userId = GetCurrentUserId();
            var result = await _service.RejectAsync(userId, id);

            return Ok(result);
        }

        /// <summary>
        /// Zakoncz zaakceptowana transakcje.
        /// </summary>
        [HttpPut("{id:guid}/finish")]
        public async Task<IActionResult> Finish(Guid id)
        {
            // Tylko wlasciciel oferty moze zakonczyc transakcje
            var userId = GetCurrentUserId();
            var result = await _service.FinishAsync(userId, id);

            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            // ClaimTypes.NameIdentifier jest ustawiany podczas generowania JWT w TokenService
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
