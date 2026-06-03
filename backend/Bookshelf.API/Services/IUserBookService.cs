using Bookshelf.API.Models.DTOs.Common;
using Bookshelf.API.Models.DTOs.UserBooks;

namespace Bookshelf.API.Services
{
    public interface IUserBookService
    {
        Task<PagedResult<UserBookDto>> GetAllAsync(
            int page = 1,
            int limit = 12,
            string? type = null,
            string? genre = null,
            string? city = null,
            string? search = null,
            string sort = "newest");

        Task<UserBookDto?> GetByIdAsync(Guid id);

        Task<List<UserBookDto>> GetRecentAsync(int limit = 5);

        Task<List<UserBookDto>> GetMyAsync(Guid userId);

        Task<UserBookDto> CreateAsync(Guid userId, CreateUserBookDto dto);

        Task<UserBookDto> UpdateAsync(Guid userId, Guid userBookId, UpdateUserBookDto dto);

        Task<UserBookDto> UploadCoverAsync(Guid userId, Guid userBookId, IFormFile file);

        Task<(byte[] Data, string ContentType)> GetCoverImageAsync(Guid userBookId);

        Task DeleteCoverAsync(Guid userId, Guid userBookId);

        Task DeleteAsync(Guid userId, Guid userBookId);
    }
}