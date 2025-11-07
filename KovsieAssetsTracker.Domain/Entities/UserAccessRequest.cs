using System.ComponentModel.DataAnnotations;

namespace KovsieAssetsTracker.Domain.Entities
{
    /// <summary>
    /// Represents a user-submitted request to gain access to the Kovsie Assets Tracker system.
    /// </summary>
    public class UserAccessRequest
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string RoleApplied { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Message { get; set; }

        /// <summary>Relative path to the uploaded PDF file stored under /files/access_forms/</summary>
        public string? PdfFilePath { get; set; }

        /// <summary>Indicates whether the access request has been approved by an administrator.</summary>
        public bool IsApproved { get; set; } = false;

        /// <summary>The name or email of the administrator who approved the request.</summary>
        public string? ApprovedBy { get; set; }

        /// <summary>The name or email of the administrator who declined the request.</summary>
        public string? DeclinedBy { get; set; }

        /// <summary>Date when the request was first submitted.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Date when the request was last updated (approval or decline).</summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
