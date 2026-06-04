using Bookshelf.API.Data;
using Bookshelf.API.Exceptions;
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
                throw new ConflictException("Użytkownik z tym adresem email już istnieje");

            // Sprawdź czy username już istnieje
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                throw new ConflictException("Ten nick jest już zajęty");

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
                throw new UnauthorizedException("Nieprawidłowy email lub hasło");

            // Wygeneruj token i zwróć
            return new AuthResponseDto
            {
                Token = _tokenService.GenerateToken(user),
                User = MapToUserDto(user)
            };
        }

        public async Task VerifyPasswordResetAsync(VerifyPasswordResetDto dto)
        {
            // Szukamy konta po danych weryfikacyjnych. Przy bledzie zawsze zwracamy ten sam komunikat.
            _ = await GetUserForPasswordResetOrThrowAsync(dto);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await GetUserForPasswordResetOrThrowAsync(dto.Verification);

            // Odpowiedz na zabawne pytanie nie jest weryfikowana - to tylko element formularza.
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();
        }

        private async Task<User> GetUserForPasswordResetOrThrowAsync(VerifyPasswordResetDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Email == dto.Email &&
                u.Username == dto.Username &&
                u.CreatedAt.Month == dto.CreatedMonth);

            if (user == null)
                throw new BadRequestException("Podane dane nie pasuja do zadnego konta");

            return user;
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
