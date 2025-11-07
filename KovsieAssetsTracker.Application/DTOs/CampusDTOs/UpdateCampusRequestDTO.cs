using Microsoft.AspNetCore.Http;

namespace KovsieAssetsTracker.Application.DTOs.CampusDTOs
{
    /// <summary>
    /// Represents the data required to update an existing campus.
    /// </summary>
    public class UpdateCampusRequestDTO
    {
        /// <summary>The unique server ID of the campus to update.</summary>
        public int CampusServerId { get; set; }

        /// <summary>The local (offline) ID used by Android clients.</summary>
        public int? CampusLocalId { get; set; }

        /// <summary>The name of the campus.</summary>
        public string CampusName { get; set; } = string.Empty;

        /// <summary>A description of the campus.</summary>
        public string CampusDescription { get; set; } = string.Empty;

        /// <summary>Indicates if this record is backed up on Android.</summary>
        public bool IsAndroidBackedUp { get; set; } = false;

        /// <summary>Indicates if this record is backed up on the server.</summary>
        public bool IsServerBackedUp { get; set; } = false;

        /// <summary>Optional updated image file for the campus.</summary>
        public IFormFile? ImageFile { get; set; }
    }
}
