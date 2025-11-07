namespace KovsieAssetsTracker.Application.DTOs.UserDTO
{
    /// <summary>Request model for changing a user’s password.</summary>
    public class ChangePasswordRequestDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
