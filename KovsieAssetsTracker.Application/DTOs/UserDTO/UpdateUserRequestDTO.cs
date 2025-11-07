using Microsoft.AspNetCore.Http;

namespace KovsieAssetsTracker.Application.DTOs.UserDTO
{
    /// <summary>Request model for updating a user’s profile.</summary>
    public class UpdateUserRequestDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string? UsernameDisplay { get; set; }
        public string? UserEmail { get; set; }
        public string? StudentStaffNumber { get; set; }
        public string? ContactNumber { get; set; }
        public string? Description { get; set; }
        public string? UserRole { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
