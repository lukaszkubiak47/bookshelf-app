using Bookshelf.API.Data;
using Bookshelf.API.Models.DTOs.Common;
using Bookshelf.API.Models.DTOs.UserBooks;
using Bookshelf.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf.API.Services
{
    public class UserBookService : IUserBookService
    {
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
                _ => query.OrderByDescending(ub => ub.CreatedAt) // "newest" - domyślne
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
                    CoverUrl = ub.Book.CoverUrl,
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