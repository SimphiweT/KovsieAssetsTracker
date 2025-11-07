using Microsoft.AspNetCore.Identity;

namespace KovsieAssetsTracker.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? UsernameDisplay { get; set; }
        public string? StudentStaffNumber { get; set; }
        public string? ProfileImageFileName { get; set; }
        public string? ContactNumber { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; } 
        public DateTime? UpdatedDate { get; set; }
    }
}
