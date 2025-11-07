using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KovsieAssetsTracker.Application.DTOs.AuthDTOs
{
    /// <summary>
    /// Represents the payload submitted when a user requests access to the system.
    /// </summary>
    public class AccessRequestDTO
    {
        /// <summary>Full name or display name of the requester.</summary>
        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Email address of the requester.</summary>
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>The role being requested (e.g., Staff, Agent, Administrator).</summary>
        [Required, MaxLength(50)]
        public string RoleApplied { get; set; } = string.Empty;

        /// <summary>Optional message or motivation for access.</summary>
        [MaxLength(500)]
        public string? Message { get; set; }

        /// <summary>The uploaded access request document (PDF format).</summary>
        public IFormFile? PdfDocument { get; set; } = default!;
    }
}
