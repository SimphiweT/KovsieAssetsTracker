using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KovsieAssetsTracker.WebAppMVC.Models.Account
{
    /// <summary>
    /// Represents a user’s access request submission (maps to AccessRequestDTO).
    /// </summary>
    public class AccessRequestViewModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Display(Name = "Requested Role")]
        public string RoleApplied { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Motivation / Message (Optional)")]
        public string? Message { get; set; }

        [Display(Name = "Attach Access Request PDF (Optional)")]
        public IFormFile? PdfDocument { get; set; }
    }
}
