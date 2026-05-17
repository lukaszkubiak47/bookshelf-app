using Bookshelf.API.Models.Entities;

namespace Bookshelf.API.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}