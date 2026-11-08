using Bookshelf.API.Models.DTOs.Transactions;
using Bookshelf.API.RateLimiting;
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
        [RateLimit("transaction-create", limit: 20, windowSeconds: 3600)] // per użytkownik
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

        /// <summary>
        /// Dane kontaktowe drugiej strony — tylko dla strony transakcji i statusu accepted/done.
        /// </summary>
        [HttpGet("{id:guid}/contact")]
        public async Task<IActionResult> GetContact(Guid id)
        {
            // Serwis sprawdza, czy pytajacy jest strona transakcji i czy status pozwala na kontakt.
            var userId = GetCurrentUserId();
            var result = await _service.GetContactAsync(userId, id);

            return Ok(result);
        }

        /// <summary>
        /// Oś czasu transakcji — tylko dla strony transakcji.
        /// </summary>
        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetHistoryAsync(userId, id);
            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            // ClaimTypes.NameIdentifier jest ustawiany podczas generowania JWT w TokenService
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
