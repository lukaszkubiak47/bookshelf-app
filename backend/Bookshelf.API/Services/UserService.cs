using Bookshelf.API.Data;
using Bookshelf.API.Exceptions;
using Bookshelf.API.Models.DTOs.Users;
using Bookshelf.API.Models.Entities;
using Bookshelf.API.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf.API.Services
{
    public class UserService : IUserService
    {
        private const int MaxAvatarSizeBytes = 2 * 1024 * 1024;
        private static readonly string[] AllowedAvatarContentTypes = ["image/jpeg", "image/png"];

        private readonly AppDbContext _db;
        private readonly IImageStorage _images;

        public UserService(AppDbContext db, IImageStorage images)
        {
            _db = db;
            _images = images;
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

            // Nowy plik do kontenera "avatars", prefiks = id użytkownika
            var previousBlob = user.AvatarBlobName;
            user.AvatarBlobName = await _images.UploadAsync("avatars", user.Id.ToString(), file);

            // Publiczny adres avatara KONKRETNEGO użytkownika (działa też na cudzych kartach)
            user.AvatarUrl = $"/api/users/{user.Id}/avatar";

            // Zapisz zmiany i zwróć zaktualizowany profil
            await _db.SaveChangesAsync();

            // Stary plik usuwamy dopiero po zapisie, żeby baza nigdy nie wskazywała na usunięty blob
            await _images.DeleteAsync("avatars", previousBlob);

            return await MapToDtoAsync(user);
        }

        public async Task<UserDto> SetAvatarPresetAsync(Guid userId, AvatarPresetDto dto)
        {
            // Pobierz użytkownika, któremu ustawiamy avatar presetowy
            var user = await GetUserOrThrowAsync(userId);

            // Ustaw ścieżkę do avatara presetowego i usuń ewentualny upload binarny
            var previousBlob = user.AvatarBlobName;
            user.AvatarUrl = $"/avatars/preset-{dto.PresetId}.png";
            user.AvatarBlobName = null;

            // Zapisz zmiany, potem usuń wgrany wcześniej plik
            await _db.SaveChangesAsync();
            await _images.DeleteAsync("avatars", previousBlob);
            return await MapToDtoAsync(user);
        }

        public async Task<Uri> GetAvatarReadUriAsync(Guid userId)
        {
            var user = await GetUserOrThrowAsync(userId);

            // Preset albo brak avatara -> nie ma pliku w Blob Storage
            if (string.IsNullOrEmpty(user.AvatarBlobName))
                throw new NotFoundException("Uzytkownik nie ma wgranego avatara");

            return _images.GetReadUri("avatars", user.AvatarBlobName);
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
    }
}
