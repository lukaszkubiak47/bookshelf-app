using Bookshelf.API.Data;
using Bookshelf.API.Exceptions;
using Bookshelf.API.Models.DTOs.Users;
using Bookshelf.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf.API.Services
{
    public class UserService : IUserService
    {
        private const int MaxAvatarSizeBytes = 2 * 1024 * 1024;
        private static readonly string[] AllowedAvatarContentTypes = ["image/jpeg", "image/png"];

        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<UserDto> GetMeAsync(Guid userId)
        {
            // Pobierz zalogowanego użytkownika z bazy
            var user = await GetUserOrThrowAsync(userId);

            // Zmapuj encję na DTO bez wrażliwych danych
            return await MapToDtoAsync(user);
        }

        public async Task<UserDto> UpdateMeAsync(Guid userId, UpdateUserDto dto)
        {
            // Pobierz użytkownika, którego profil edytujemy
            var user = await GetUserOrThrowAsync(userId);

            // Jeśli zmieniany jest nick, sprawdź czy nie zajął go ktoś inny
            if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.Username)
            {
                var usernameExists = await _db.Users.AnyAsync(u =>
                    u.Username == dto.Username && u.Id != userId);

                if (usernameExists)
                    throw new ConflictException("Ten nick jest juz zajety");

                user.Username = dto.Username;
            }

            // Aktualizuj tylko pola, które przyszły w request body
            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.City))
                user.City = dto.City;

            // Zapisz zmiany i zwróć aktualny profil
            await _db.SaveChangesAsync();
            return await MapToDtoAsync(user);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            // Pobierz użytkownika, któremu zmieniamy hasło
            var user = await GetUserOrThrowAsync(userId);

            // Sprawdź czy obecne hasło pasuje do hasha w bazie
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedException("Nieprawidlowe aktualne haslo");

            // Zahashuj nowe hasło i zapisz je w bazie
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();
        }

        public async Task<UserDto> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            // Pobierz użytkownika, dla którego zapisujemy avatar
            var user = await GetUserOrThrowAsync(userId);

            // Sprawdź czy plik faktycznie został wysłany
            if (file == null || file.Length == 0)
                throw new BadRequestException("Plik jest wymagany");

            // Ogranicz rozmiar avatara do 2 MB zgodnie ze specyfikacją
            if (file.Length > MaxAvatarSizeBytes)
                throw new BadRequestException("Plik nie moze przekraczac 2 MB");

            // Przyjmujemy tylko JPG i PNG
            if (!AllowedAvatarContentTypes.Contains(file.ContentType))
                throw new BadRequestException("Dozwolone formaty avatara: JPG, PNG");

            // Przepisz przesłany plik do tablicy bajtów
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            // Zapisz dane binarne avatara i ustaw URL na endpoint pobierający obraz
            user.AvatarImageData = memoryStream.ToArray();
            user.AvatarUrl = "/api/users/me/avatar/image";

            // Zapisz zmiany i zwróć zaktualizowany profil
            await _db.SaveChangesAsync();
            return await MapToDtoAsync(user);
        }

        public async Task<UserDto> SetAvatarPresetAsync(Guid userId, AvatarPresetDto dto)
        {
            // Pobierz użytkownika, któremu ustawiamy avatar presetowy
            var user = await GetUserOrThrowAsync(userId);

            // Ustaw ścieżkę do avatara presetowego i usuń ewentualny upload binarny
            user.AvatarUrl = $"/avatars/preset-{dto.PresetId}.png";
            user.AvatarImageData = null;

            // Zapisz zmiany i zwróć zaktualizowany profil
            await _db.SaveChangesAsync();
            return await MapToDtoAsync(user);
        }

        public async Task<(byte[] Data, string ContentType)> GetAvatarImageAsync(Guid userId)
        {
            // Pobierz użytkownika, którego avatar ma zostać zwrócony jako plik
            var user = await GetUserOrThrowAsync(userId);

            // Jeśli użytkownik wybrał preset albo nie ma avatara, nie mamy danych binarnych do zwrócenia
            if (user.AvatarImageData == null || user.AvatarImageData.Length == 0)
                throw new NotFoundException("Uzytkownik nie ma wgranego avatara");

            // Zwróć bajty obrazu razem z typem MIME
            return (user.AvatarImageData, GetAvatarContentType(user.AvatarImageData));
        }

        private async Task<User> GetUserOrThrowAsync(Guid userId)
        {
            // Wspólne pobieranie użytkownika dla wszystkich metod serwisu
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // Jeśli token wskazuje na nieistniejącego użytkownika, zwracamy 404
            if (user == null)
                throw new NotFoundException("Nie znaleziono uzytkownika");

            return user;
        }

        private async Task<UserDto> MapToDtoAsync(User user)
        {
            // Policz wystawione książki użytkownika
            var booksCount = await _db.UserBooks.CountAsync(ub => ub.UserId == user.Id);

            // Policz transakcje wysłane przez użytkownika
            var transactionsCount = await _db.TransactionRequests.CountAsync(tr => tr.RequesterId == user.Id);

            // Zwróć DTO bez PasswordHash
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                City = user.City,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                BooksCount = booksCount,
                TransactionsCount = transactionsCount
            };
        }

        private static string GetAvatarContentType(byte[] data)
        {
            // Content-Type nie ma osobnej kolumny w bazie, więc rozpoznajemy PNG po sygnaturze pliku
            if (data.Length >= 8 &&
                data[0] == 0x89 &&
                data[1] == 0x50 &&
                data[2] == 0x4E &&
                data[3] == 0x47)
            {
                return "image/png";
            }

            // Jeśli to nie PNG, traktujemy plik jako JPG, bo tylko JPG/PNG przechodzą walidację uploadu
            return "image/jpeg";
        }
    }
}
