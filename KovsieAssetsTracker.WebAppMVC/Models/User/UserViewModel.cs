namespace KovsieAssetsTracker.WebAppMVC.Models.User
{
    public class UserViewModel
    {
        public string? UserId { get; set; }
        public string? UsernameDisplay { get; set; }
        public string? UserEmail { get; set; }
        public string? StudentStaffNumber { get; set; }
        public string? ProfileImageFileName { get; set; }
        public string? ContactNumber { get; set; }
        public string? Description { get; set; }
        public string? UserRole { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
