namespace KovsieAssetsTracker.Application.DTOs.AuthDTOs
{
    /// <summary>
    /// Represents the response returned for access request data.
    /// </summary>
    public class AccessResponseDTO
    {
        public int Id { get; set; }

        /// <summary>Full name of the requester.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Email address of the requester.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>The role the user applied for.</summary>
        public string RequestedRole { get; set; } = string.Empty;

        /// <summary>URL path to the uploaded PDF document.</summary>
        public string DocumentPath { get; set; } = string.Empty;

        /// <summary>Current status of the request (Pending / Approved / Declined).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Timestamp when the request was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Timestamp when the request was reviewed, if applicable.</summary>
        public DateTime? ReviewedAt { get; set; }
    }
}
