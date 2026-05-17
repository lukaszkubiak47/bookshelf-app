using Bookshelf.API.Data;
using Bookshelf.API.Models.DTOs.Auth;
using Bookshelf.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Sprawdź czy email już istnieje
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Użytkownik z tym adresem email już istnieje");

            // Sprawdź czy username już istnieje
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                throw new InvalidOperationException("Ten nick jest już zajęty");

            // Stwórz nowego użytkownika z zahashowanym hasłem
            var user = new User
            {
                Email = dto.Email,
                Username = dto.Username,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                City = dto.City,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Wygeneruj token i zwróć
            return new AuthResponseDto
            {
                Token = _tokenService.GenerateToken(user),
                User = MapToUserDto(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // Znajdź użytkownika po emailu
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Sprawdź czy istnieje i czy hasło się zgadza
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Nieprawidłowy email lub hasło");

            // Wygeneruj token i zwróć
            return new AuthResponseDto
            {
                Token = _tokenService.GenerateToken(user),
                User = MapToUserDto(user)
            };
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                City = user.City,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt
            };
        }
    }
}