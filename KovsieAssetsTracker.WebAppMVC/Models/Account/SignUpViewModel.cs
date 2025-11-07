using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KovsieAssetsTracker.WebAppMVC.Models.Account
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string DisplayName { get; set; } = string.Empty;

        [Required, EmailAddress(ErrorMessage = "Valid email is required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select a role")]
        public string Role { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Message { get; set; }

        //[Required(ErrorMessage = "Please upload a PDF document")]
        public IFormFile? Document { get; set; }
    }
}
