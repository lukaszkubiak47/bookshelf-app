using Bookshelf.API.Data;
using Bookshelf.API.Exceptions;
using Bookshelf.API.Models.DTOs.Common;
using Bookshelf.API.Models.DTOs.UserBooks;
using Bookshelf.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf.API.Services
{
    public class UserBookService : IUserBookService
    {
        private const int MaxCoverSizeBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedCoverContentTypes = ["image/jpeg", "image/png"];

        private readonly AppDbContext _db;

        public UserBookService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<UserBookDto>> GetAllAsync(
            int page = 1,
            int limit = 12,
            string? type = null,
            string? genre = null,
            string? city = null,
            string? search = null,
            string sort = "newest")
        {
            // Bazowe query z dołączonymi relacjami
            var query = _db.UserBooks
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Genre)
                .Include(ub => ub.User)
                .AsQueryable();

            // Pokazujemy tylko oferty dostępne — wypożyczone/zarezerwowane znikają z listy do czasu zakończenia transakcji.
            query = query.Where(ub => ub.Status == "available");

            // Filtry
            if (!string.IsNullOrWhiteSpace(type))
            {
                // type może być listą po przecinku: "loan,swap"
                var types = type.Split(',', StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(ub => types.Contains(ub.TransactionType));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(ub => ub.Book.Genre != null && ub.Book.Genre.Name == genre);
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(ub => ub.User.City.Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(ub =>
                    ub.Book.Title.Contains(search) ||
                    ub.Book.Author.Contains(search) ||
                    (ub.Book.ISBN != null && ub.Book.ISBN.Contains(search)));
            }

            // Sortowanie
            query = sort switch
            {
                "oldest" => query.OrderBy(ub => ub.CreatedAt),
                "title" => query.OrderBy(ub => ub.Book.Title),
                _ => query.OrderByDescending(ub => ub.CreatedAt) //domyśle "newest"
            };

            // Liczenie i paginacja
            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(ub => MapToDto(ub))
                .ToListAsync();

            return new PagedResult<UserBookDto>
            {
                Items = items,
                Total = total,
                Page = page,
                Limit = limit
            };
        }

        public async Task<UserBookDto?> GetByIdAsync(Guid id)
        {
            var userBook = await _db.UserBooks
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Genre)
                .Include(ub => ub.User)
                .FirstOrDefaultAsync(ub => ub.Id == id);

            return userBook == null ? null : MapToDto(userBook);
        }

        public async Task<List<UserBookDto>> GetRecentAsync(int limit = 5)
        {
            return await _db.UserBooks
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Genre)
                .Include(ub => ub.User)
                // Tylko dostępne oferty (spójnie z "Przeglądaj" i statystyką activeOffers).
                .Where(ub => ub.Status == "available")
                .OrderByDescending(ub => ub.CreatedAt)
                .Take(limit)
                .Select(ub => MapToDto(ub))
                .ToListAsync();
        }

        public async Task<List<UserBookDto>> GetMyAsync(Guid userId)
        {
            return await _db.UserBooks
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Genre)
                .Include(ub => ub.User)
                .Where(ub => ub.UserId == userId)
                .OrderByDescending(ub => ub.CreatedAt)
                .Select(ub => MapToDto(ub))
                .ToListAsync();
        }


        public async Task<UserBookDto> CreateAsync(Guid userId, CreateUserBookDto dto)
        {
            // Sprawdź czy książka istnieje w bazie
            var book = await _db.Books
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.Id == dto.BookId);

            if (book == null)
                throw new NotFoundException("Nie znaleziono książki o podanym ID");

            // Walidacja: cena wymagana tylko dla sprzedaży
            if (dto.TransactionType == "sale" && dto.Price == null)
                throw new BadRequestException("Dla sprzedaży wymagana jest cena");

            // Walidacja: cena tylko dla sprzedaży
            if (dto.TransactionType != "sale" && dto.Price != null)
                throw new BadRequestException("Cena jest dozwolona tylko dla sprzedaży");

            // Walidacja: warunki wymiany tylko dla swap
            if (dto.TransactionType != "swap" && !string.IsNullOrEmpty(dto.ExchangeCondition))
                throw new BadRequestException("Warunki wymiany są dozwolone tylko dla wymiany");

            // Tworzenie nowej oferty
            var userBook = new UserBook
            {
                UserId = userId,
                BookId = dto.BookId,
                Status = "available",
                TransactionType = dto.TransactionType,
                Price = dto.Price,
                ExchangeCondition = dto.ExchangeCondition,
                ExchangeDetails = dto.ExchangeDetails
            };

            _db.UserBooks.Add(userBook);
            await _db.SaveChangesAsync();

            // Pobranie oferty pełnymi danymi (Book + User + Genre) do zwrotu
            var created = await _db.UserBooks
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Genre)
                .Include(ub => ub.User)
                .FirstAsync(ub => ub.Id == userBook.Id);

            return MapToDto(created);
        }

        public async Task<UserBookDto> UpdateAsync(Guid userId, Guid userBookId, UpdateUserBookDto dto)
        {
            // Pobierz ofertę z bazy
            var userBook = await _db.UserBooks
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Genre)
                .Include(ub => ub.User)
                .FirstOrDefaultAsync(ub => ub.Id == userBookId);

            if (userBook == null)
                throw new NotFoundException("Nie znaleziono oferty o podanym ID");

            // KRYTYCZNE: sprawdź czy zalogowany user jest właścicielem
            if (userBook.UserId != userId)
                throw new ForbiddenException("Możesz edytować tylko swoje oferty");

            // Docelowy typ transakcji (nowy lub stary jeśli nie zmieniany)
            var newType = dto.TransactionType ?? userBook.TransactionType;
            var newPrice = dto.Price ?? userBook.Price;
            var newExchangeCondition = dto.ExchangeCondition ?? userBook.ExchangeCondition;

            // Walidacje biznesowe (te same co przy Create, ale dla nowych wartości)
            if (newType == "sale" && newPrice == null)
                throw new BadRequestException("Dla sprzedaży wymagana jest cena");

            if (newType != "sale" && newPrice != null)
                throw new BadRequestException("Cena jest dozwolona tylko dla sprzedaży");

            if (newType != "swap" && !string.IsNullOrEmpty(newExchangeCondition))
                throw new BadRequestException("Warunki wymiany są dozwolone tylko dla wymiany");

            // Zastosuj zmiany
            if (dto.TransactionType != null)
                userBook.TransactionType = dto.TransactionType;

            if (dto.Price != null)
                userBook.Price = dto.Price;

            if (dto.ExchangeCondition != null)
                userBook.ExchangeCondition = dto.ExchangeCondition;

            if (dto.ExchangeDetails != null)
                userBook.ExchangeDetails = dto.ExchangeDetails;

            if (dto.BookDescription != null)
                userBook.Book.Description = string.IsNullOrWhiteSpace(dto.BookDescription)
                    ? null
                    : dto.BookDescription.Trim();

            // Specjalne czyszczenie: jeśli zmieniono typ, wyczyść nieadekwatne pola
            if (dto.TransactionType != null)
            {
                if (userBook.TransactionType != "sale")
                    userBook.Price = null;

                if (userBook.TransactionType != "swap")
                {
                    userBook.ExchangeCondition = null;
                    userBook.ExchangeDetails = null;
                }
            }

            await _db.SaveChangesAsync();

            return MapToDto(userBook);
        }

        public async Task<UserBookDto> UploadCoverAsync(Guid userId, Guid userBookId, IFormFile file)
        {
            var userBook = await GetOwnedUserBookAsync(userId, userBookId);

            if (file == null || file.Length == 0)
                throw new BadRequestException("Plik jest wymagany");

            if (file.Length > MaxCoverSizeBytes)
                throw new BadRequestException("Plik nie może przekraczać 5 MB");

            if (!AllowedCoverContentTypes.Contains(file.ContentType))
                throw new BadRequestException("Dozwolone formaty okładki: JPG, PNG");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            userBook.CustomCoverImageData = memoryStream.ToArray();

            await _db.SaveChangesAsync();
            return MapToDto(userBook);
        }

        public async Task<(byte[] Data, string ContentType)> GetCoverImageAsync(Guid userBookId)
        {
            var userBook = await _db.UserBooks
                .AsNoTracking()
                .FirstOrDefaultAsync(ub => ub.Id == userBookId);

            if (userBook?.CustomCoverImageData == null || userBook.CustomCoverImageData.Length == 0)
                throw new NotFoundException("Oferta nie ma własnej okładki");

            return (userBook.CustomCoverImageData, GetImageContentType(userBook.CustomCoverImageData));
        }

        public async Task DeleteCoverAsync(Guid userId, Guid userBookId)
        {
            var userBook = await GetOwnedUserBookAsync(userId, userBookId);
            userBook.CustomCoverImageData = null;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid userId, Guid userBookId)
        {
            // Pobierz ofertę z bazy (z transakcjami żeby sprawdzić czy są aktywne)
            var userBook = await _db.UserBooks
                .Include(ub => ub.TransactionRequests)
                .FirstOrDefaultAsync(ub => ub.Id == userBookId);

            if (userBook == null)
                throw new NotFoundException("Nie znaleziono oferty o podanym ID");

            // KRYTYCZNE: sprawdź czy zalogowany user jest właścicielem
            if (userBook.UserId != userId)
                throw new ForbiddenException("Możesz usuwać tylko swoje oferty");

            // Sprawdź czy nie ma aktywnej transakcji
            var hasActiveTransaction = userBook.TransactionRequests
                .Any(tr => tr.Status == "accepted");

            if (hasActiveTransaction)
                throw new ConflictException("Nie można usunąć oferty z aktywną transakcją. Najpierw ją zakończ.");

            _db.UserBooks.Remove(userBook);
            await _db.SaveChangesAsync();
        }

        private async Task<UserBook> GetOwnedUserBookAsync(Guid userId, Guid userBookId)
        {
            var userBook = await _db.UserBooks
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Genre)
                .Include(ub => ub.User)
                .FirstOrDefaultAsync(ub => ub.Id == userBookId);

            if (userBook == null)
                throw new NotFoundException("Nie znaleziono oferty o podanym ID");

            if (userBook.UserId != userId)
                throw new ForbiddenException("Możesz edytować tylko swoje oferty");

            return userBook;
        }

        private static string? ResolveCoverUrl(UserBook ub)
        {
            if (ub.CustomCoverImageData != null && ub.CustomCoverImageData.Length > 0)
                return $"/api/userbooks/{ub.Id}/cover/image";

            return ub.Book.CoverUrl;
        }

        private static string GetImageContentType(byte[] data)
        {
            if (data.Length >= 8 &&
                data[0] == 0x89 &&
                data[1] == 0x50 &&
                data[2] == 0x4E &&
                data[3] == 0x47)
            {
                return "image/png";
            }

            return "image/jpeg";
        }

        private static UserBookDto MapToDto(UserBook ub)
        {
            return new UserBookDto
            {
                Id = ub.Id,
                Status = ub.Status,
                TransactionType = ub.TransactionType,
                Price = ub.Price,
                ExchangeCondition = ub.ExchangeCondition,
                ExchangeDetails = ub.ExchangeDetails,
                HasCustomCover = ub.CustomCoverImageData != null,
                CreatedAt = ub.CreatedAt,
                Book = new BookInfoDto
                {
                    Id = ub.Book.Id,
                    Title = ub.Book.Title,
                    Author = ub.Book.Author,
                    Description = ub.Book.Description,
                    CoverUrl = ResolveCoverUrl(ub),
                    ISBN = ub.Book.ISBN,
                    GenreName = ub.Book.Genre?.Name
                },
                Owner = new OwnerInfoDto
                {
                    Id = ub.User.Id,
                    Username = ub.User.Username,
                    City = ub.User.City,
                    AvatarUrl = ub.User.AvatarUrl
                }
            };
        }
    }
}