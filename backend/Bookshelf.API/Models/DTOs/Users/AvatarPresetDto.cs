using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Users
{
    /// <summary>
    /// Request do wyboru avatara z gotowych presetow. Dozwolone sa presety 1-10.
    /// </summary>
    public class AvatarPresetDto
    {
        [Range(1, 10, ErrorMessage = "PresetId musi byc w zakresie 1-10")]
        public int PresetId { get; set; }
    }
}
