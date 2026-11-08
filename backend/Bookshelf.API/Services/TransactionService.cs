using Bookshelf.API.Data;
using Bookshelf.API.Exceptions;
using Bookshelf.API.Models.DTOs.Transactions;
using Bookshelf.API.Models.Entities;
using Bookshelf.API.Models.Events;
using Bookshelf.API.Services.Events;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf.API.Services
{
    public class TransactionService : ITransactionService
    {
        private const string PendingStatus = "pending";
        private const string AcceptedStatus = "accepted";
        private const string RejectedStatus = "rejected";
        private const string DoneStatus = "done";

        private const string AvailableStatus = "available";
        private const string BorrowedStatus = "borrowed";
        private const string ReservedStatus = "reserved";
        private const string CompletedStatus = "completed";
        private const string SoldStatus = "sold";

        private readonly AppDbContext _db;
        private readonly IEventPublisher _events;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(AppDbContext db, IEventPublisher events, ILogger<TransactionService> logger)
        {
            _db = db;
            _events = events;
            _logger = logger;
        }

        public async Task<TransactionDto> CreateAsync(Guid requesterId, CreateTransactionDto dto)
        {
            // Pobierz ofertę razem z książką i właścicielem, bo te dane wrócą w DTO
            var userBook = await _db.UserBooks
                .Include(ub => ub.Book)
                .Include(ub => ub.User)
                .FirstOrDefaultAsync(ub => ub.Id == dto.UserBookId);

            if (userBook == null)
                throw new NotFoundException("Nie znaleziono oferty o podanym ID");

            // Użytkownik nie może wysłać prośby o własną książkę
            if (userBook.UserId == requesterId)
                throw new BadRequestException("Nie możesz wysłać prośby o własną książkę");

            // Prośby przyjmujemy tylko dla dostępnych ofert
            if (userBook.Status != AvailableStatus)
                throw new ConflictException("Ta oferta nie jest już dostępna");

            // Jedna zaakceptowana prośba na ofertę — dopóki trwa transakcja, oferta znika z przeglądania
            var hasAccepted = await _db.TransactionRequests.AnyAsync(tr =>
                tr.UserBookId == dto.UserBookId && tr.Status == AcceptedStatus);

            if (hasAccepted)
                throw new ConflictException("Ta oferta nie jest już dostępna");

            var hasCompleted = await _db.TransactionRequests.AnyAsync(tr =>
                tr.UserBookId == dto.UserBookId && tr.Status == DoneStatus);

            if (hasCompleted)
                throw new ConflictException("Ta oferta została już zrealizowana");

            // Ten sam użytkownik nie może mieć drugiej aktywnej prośby pending na tę samą ofertę
            var pendingExists = await _db.TransactionRequests.AnyAsync(tr =>
                tr.UserBookId == dto.UserBookId &&
                tr.RequesterId == requesterId &&
                tr.Status == PendingStatus);

            if (pendingExists)
                throw new ConflictException("Masz już wysłaną aktywną prośbę na tę ofertę");

            // Typ transakcji dziedziczymy z oferty, a nie z requestu
            var transaction = new TransactionRequest
            {
                RequesterId = requesterId,
                UserBookId = dto.UserBookId,
                Type = userBook.TransactionType,
                Status = PendingStatus,
                Message = dto.Message,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.TransactionRequests.Add(transaction);
            await _db.SaveChangesAsync();

            // Pobierz transakcję ponownie z relacjami potrzebnymi do mapowania DTO
            var created = await GetTransactionWithDetailsOrThrowAsync(transaction.Id);

            // Requested: od proszącego do właściciela
            await PublishSafeAsync(BuildEvent("Requested", created, requesterId, userBook.UserId));

            return MapToDto(created);
        }

        public async Task<List<TransactionDto>> GetReceivedAsync(Guid ownerId)
        {
            // Prośby otrzymane to te, które dotyczą książek aktualnego właściciela
            var transactions = await BuildTransactionDetailsQuery()
                .Where(t => t.UserBook.UserId == ownerId)
                .OrderBy(t => t.Status == PendingStatus ? 0 : 1)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            return transactions.Select(MapToDto).ToList();
        }

        public async Task<List<TransactionDto>> GetSentAsync(Guid requesterId)
        {
            // Prośby wysłane to te, gdzie aktualny użytkownik jest requesterem
            var transactions = await BuildTransactionDetailsQuery()
                .Where(t => t.RequesterId == requesterId)
                .OrderBy(t => t.Status == PendingStatus ? 0 : 1)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            return transactions.Select(MapToDto).ToList();
        }

        public async Task<TransactionContactDto> GetContactAsync(Guid currentUserId, Guid transactionId)
        {
            var transaction = await BuildTransactionDetailsQuery()
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            // Brak transakcji LUB pytający nie jest jej stroną -> 404 (nie zdradzamy, że transakcja istnieje).
            if (transaction == null)
                throw new NotFoundException("Nie znaleziono transakcji");

            var requesterId = transaction.RequesterId;
            var ownerId = transaction.UserBook.UserId;

            if (currentUserId != requesterId && currentUserId != ownerId)
                throw new NotFoundException("Nie znaleziono transakcji");

            // Strona transakcji, ale przed akceptacją (pending/rejected) -> 403 z jasnym komunikatem.
            if (transaction.Status != AcceptedStatus && transaction.Status != DoneStatus)
                throw new ForbiddenException("Dane kontaktowe dostępne po zaakceptowaniu prośby");

            // Zwracamy dane DRUGIEJ strony, nigdy własne.
            var other = currentUserId == requesterId ? transaction.UserBook.User : transaction.Requester;

            return new TransactionContactDto
            {
                Username = other.Username,
                Email = other.Email
            };
        }

        public async Task<TransactionDto> AcceptAsync(Guid ownerId, Guid transactionId, AcceptTransactionDto dto)
        {
            // Pobierz transakcję i sprawdź, czy aktualny użytkownik jest właścicielem oferty
            var transaction = await GetOwnedTransactionOrThrowAsync(transactionId, ownerId);

            if (transaction.Status != PendingStatus)
                throw new ConflictException("Można zaakceptować tylko prośbę o statusie pending");

            if (transaction.UserBook.Status != AvailableStatus)
                throw new ConflictException("Ta oferta nie jest już dostępna");

            var alreadyAccepted = await _db.TransactionRequests.AnyAsync(tr =>
                tr.UserBookId == transaction.UserBookId && tr.Status == AcceptedStatus);

            if (alreadyAccepted)
                throw new ConflictException("Ta oferta ma już zaakceptowaną prośbę");

            // Zmień status prośby na accepted
            transaction.Status = AcceptedStatus;
            transaction.UpdatedAt = DateTime.UtcNow;

            if (dto.DueDate != null)
                transaction.DueDate = dto.DueDate;

            if (!string.IsNullOrWhiteSpace(dto.Message))
                transaction.Message = dto.Message;

            var offerStatus = transaction.Type == "sale" ? ReservedStatus : BorrowedStatus;

            // Wszystkie pozostałe pending dla tej samej oferty automatycznie odrzucamy
            var otherPendingTransactions = await _db.TransactionRequests
                .Where(tr =>
                    tr.UserBookId == transaction.UserBookId &&
                    tr.Id != transaction.Id &&
                    tr.Status == PendingStatus)
                .ToListAsync();

            foreach (var other in otherPendingTransactions)
            {
                other.Status = RejectedStatus;
                other.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            await SetOfferStatusAsync(transaction.UserBookId, offerStatus);

            // Zdarzenia dopiero po obu zapisach (prośby + status oferty)
            await PublishSafeAsync(BuildEvent("Accepted", transaction, ownerId, transaction.RequesterId));
            foreach (var other in otherPendingTransactions)
                await PublishSafeAsync(BuildEvent("AutoRejectedOnAccept", other, ownerId, other.RequesterId, transaction.UserBook.Book.Title));

            var updated = await GetTransactionWithDetailsOrThrowAsync(transaction.Id);
            return MapToDto(updated);
        }

        public async Task<TransactionDto> RejectAsync(Guid ownerId, Guid transactionId)
        {
            // Pobierz transakcję i sprawdź uprawnienia właściciela
            var transaction = await GetOwnedTransactionOrThrowAsync(transactionId, ownerId);

            if (transaction.Status != PendingStatus)
                throw new ConflictException("Można odrzucić tylko prośbę o statusie pending");

            // Odrzucenie nie zmienia statusu oferty, inni użytkownicy nadal mogą wysyłać prośby
            transaction.Status = RejectedStatus;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Rejected: od właściciela do proszącego
            await PublishSafeAsync(BuildEvent("Rejected", transaction, ownerId, transaction.RequesterId));

            var updated = await GetTransactionWithDetailsOrThrowAsync(transaction.Id);
            return MapToDto(updated);
        }

        public async Task<TransactionDto> FinishAsync(Guid ownerId, Guid transactionId)
        {
            // Pobierz transakcję i sprawdź uprawnienia właściciela
            var transaction = await GetOwnedTransactionOrThrowAsync(transactionId, ownerId);

            if (transaction.Status != AcceptedStatus)
                throw new ConflictException("Można zakończyć tylko zaakceptowaną transakcję");

            transaction.Status = DoneStatus;
            transaction.UpdatedAt = DateTime.UtcNow;

            var offerStatus = transaction.Type == "sale" ? SoldStatus : CompletedStatus;

            var otherPending = await _db.TransactionRequests
                .Where(tr =>
                    tr.UserBookId == transaction.UserBookId &&
                    tr.Id != transaction.Id &&
                    tr.Status == PendingStatus)
                .ToListAsync();

            foreach (var other in otherPending)
            {
                other.Status = RejectedStatus;
                other.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            await SetOfferStatusAsync(transaction.UserBookId, offerStatus);

            // Zdarzenia dopiero po obu zapisach (prośby + status oferty)
            await PublishSafeAsync(BuildEvent("Finished", transaction, ownerId, transaction.RequesterId));
            foreach (var other in otherPending)
                await PublishSafeAsync(BuildEvent("AutoRejectedOnFinish", other, ownerId, other.RequesterId, transaction.UserBook.Book.Title));

            var updated = await GetTransactionWithDetailsOrThrowAsync(transaction.Id);
            return MapToDto(updated);
        }

        // Błąd kolejki nie może zepsuć akcji użytkownika: zmiana w bazie już jest zapisana, tylko logujemy ostrzeżenie
        private async Task PublishSafeAsync(TransactionEvent evt)
        {
            try { await _events.PublishAsync(evt); }
            catch (Exception ex) { _logger.LogWarning(ex, "Nie udało się wysłać zdarzenia {Type}", evt.Type); }
        }

        // bookTitle podajemy dla automatycznie odrzuconych próśb, bo ich UserBook nie jest wczytany
        private static TransactionEvent BuildEvent(string type, TransactionRequest transaction, Guid actorUserId, Guid recipientUserId, string? bookTitle = null)
        {
            return new TransactionEvent(
                EventId: Guid.NewGuid(),
                TransactionId: transaction.Id,
                Type: type,
                ActorUserId: actorUserId,
                RecipientUserId: recipientUserId,
                UserBookId: transaction.UserBookId,
                BookTitle: bookTitle ?? transaction.UserBook?.Book?.Title,
                OccurredAt: DateTime.UtcNow);
        }

        private async Task SetOfferStatusAsync(Guid userBookId, string status)
        {
            var rows = await _db.UserBooks
                .Where(ub => ub.Id == userBookId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(ub => ub.Status, status));

            if (rows == 0)
                throw new NotFoundException("Nie znaleziono oferty");
        }

        private IQueryable<TransactionRequest> BuildTransactionDetailsQuery()
        {
            // Wspólne query do pobierania transakcji z pełnymi danymi do DTO
            return _db.TransactionRequests
                .Include(t => t.UserBook)
                    .ThenInclude(ub => ub.Book)
                .Include(t => t.UserBook)
                    .ThenInclude(ub => ub.User)
                .Include(t => t.Requester);
        }

        private async Task<TransactionRequest> GetTransactionWithDetailsOrThrowAsync(Guid transactionId)
        {
            // Pobierz pojedynczą transakcję z relacjami potrzebnymi do mapowania
            var transaction = await BuildTransactionDetailsQuery()
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
                throw new NotFoundException("Nie znaleziono transakcji");

            return transaction;
        }

        private async Task<TransactionRequest> GetOwnedTransactionOrThrowAsync(Guid transactionId, Guid ownerId)
        {
            // Pobierz transakcję z ofertą i właścicielem
            var transaction = await BuildTransactionDetailsQuery()
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
                throw new NotFoundException("Nie znaleziono transakcji");

            // Tylko właściciel oferty może akceptować, odrzucać i kończyć transakcję
            if (transaction.UserBook.UserId != ownerId)
                throw new ForbiddenException("Tylko właściciel książki może wykonać tę akcję");

            return transaction;
        }

        private static TransactionDto MapToDto(TransactionRequest transaction)
        {
            // Mapowanie encji na DTO, bez zwracania encji EF bezpośrednio do klienta
            return new TransactionDto
            {
                Id = transaction.Id,
                Type = transaction.Type,
                Status = transaction.Status,
                Message = transaction.Message,
                DueDate = transaction.DueDate,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                UserBook = new UserBookSummaryDto
                {
                    Id = transaction.UserBook.Id,
                    Status = transaction.UserBook.Status,
                    TransactionType = transaction.UserBook.TransactionType,
                    Price = transaction.UserBook.Price,
                    Book = new BookSummaryDto
                    {
                        Id = transaction.UserBook.Book.Id,
                        Title = transaction.UserBook.Book.Title,
                        Author = transaction.UserBook.Book.Author,
                        CoverUrl = transaction.UserBook.Book.CoverUrl
                    },
                    Owner = new UserSummaryDto
                    {
                        Id = transaction.UserBook.User.Id,
                        Username = transaction.UserBook.User.Username,
                        City = transaction.UserBook.User.City,
                        AvatarUrl = transaction.UserBook.User.AvatarUrl
                    }
                },
                Requester = new UserSummaryDto
                {
                    Id = transaction.Requester.Id,
                    Username = transaction.Requester.Username,
                    City = transaction.Requester.City,
                    AvatarUrl = transaction.Requester.AvatarUrl
                }
            };
        }
    }
}
