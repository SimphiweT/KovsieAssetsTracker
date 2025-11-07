namespace KovsieAssetsTracker.WebAppMVC.Models.Account
{
    public class LoginResponseViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpirationTime { get; set; }
    }
}
