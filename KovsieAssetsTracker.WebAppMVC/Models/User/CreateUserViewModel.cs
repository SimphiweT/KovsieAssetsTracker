namespace KovsieAssetsTracker.WebAppMVC.Models.User
{
    public class CreateUserViewModel
    {
        public string UsernameDisplay { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? StudentStaffNumber { get; set; }
        public string? ContactNumber { get; set; }
        public string? Description { get; set; }
        public string? UserRole { get; set; }
        public string Password { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
    }
}
