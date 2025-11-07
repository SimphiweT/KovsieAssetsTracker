namespace KovsieAssetsTracker.WebAppMVC.Models.User
{
    public class UpdateUserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? UsernameDisplay { get; set; }
        public string? UserEmail { get; set; }
        public string? StudentStaffNumber { get; set; }
        public string? ContactNumber { get; set; }
        public string? Description { get; set; }
        public string? UserRole { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
