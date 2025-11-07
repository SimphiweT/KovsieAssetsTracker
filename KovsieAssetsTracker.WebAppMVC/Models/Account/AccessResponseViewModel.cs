namespace KovsieAssetsTracker.WebAppMVC.Models.Account
{
    /// <summary>
    /// Represents an access request record returned by the API (maps to AccessResponseDTO).
    /// Used for admin views, dashboards, and reports.
    /// </summary>
    public class AccessResponseViewModel
    {
        public int Id { get; set; }

        /// <summary>Full name of the requester.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Email address of the requester.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>The role the user applied for.</summary>
        public string RequestedRole { get; set; } = string.Empty;

        /// <summary>URL to the uploaded PDF access form.</summary>
        public string DocumentPath { get; set; } = string.Empty;

        /// <summary>Status of the request (Pending, Approved, Declined).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Date of submission.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Date when reviewed (if applicable).</summary>
        public DateTime? ReviewedAt { get; set; }
    }
}
