using Microsoft.AspNetCore.Http;

namespace KovsieAssetsTracker.Application.DTOs.UserDTO
{
    public class CreateUserRequestDTO
    {
        public string UsernameDisplay { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? StudentStaffNumber { get; set; }
        public string? ContactNumber { get; set; }
        public string? Description { get; set; }
        public string? UserRole { get; set; }
        public string Password { get; set; } =string.Empty;
        public IFormFile? ImageFile { get; set; }
    }
}

