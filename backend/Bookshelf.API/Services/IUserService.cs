using Bookshelf.API.Models.DTOs.Users;

namespace Bookshelf.API.Services
{
    public interface IUserService
    {
        // Pobiera profil zalogowanego uzytkownika razem z licznikami do widoku profilu.
        Task<UserDto> GetMeAsync(Guid userId);

        // Aktualizuje podstawowe dane profilu: nick, imie, nazwisko i miasto.
        Task<UserDto> UpdateMeAsync(Guid userId, UpdateUserDto dto);

        // Zmienia haslo po sprawdzeniu aktualnego hasla.
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

        // Zapisuje wlasny avatar jako dane binarne w bazie.
        Task<UserDto> UploadAvatarAsync(Guid userId, IFormFile file);

        // Ustawia avatar z presetow i czysci ewentualny avatar binarny.
        Task<UserDto> SetAvatarPresetAsync(Guid userId, AvatarPresetDto dto);

        // Zwraca dane binarne avatara oraz Content-Type do odpowiedzi plikowej.
        Task<Uri> GetAvatarReadUriAsync(Guid userId);
    }
}
