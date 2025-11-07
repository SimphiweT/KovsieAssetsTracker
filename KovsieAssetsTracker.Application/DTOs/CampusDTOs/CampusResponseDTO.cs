using KovsieAssetsTracker.Application.DTOs.BuildingDTOs;

namespace KovsieAssetsTracker.Application.DTOs.CampusDTOs
{
    /// <summary>
    /// Represents the data returned when retrieving campus information.
    /// </summary>
    public class CampusResponseDTO
    {
        /// <summary>The unique ID of the campus on the server.</summary>
        public int CampusServerId { get; set; }

        /// <summary>The local (offline) ID used by Android clients.</summary>
        public int? CampusLocalId { get; set; }

        /// <summary>The name of the campus.</summary>
        public string CampusName { get; set; } = string.Empty;

        /// <summary>A brief description of the campus.</summary>
        public string CampusDescription { get; set; } = string.Empty;

        /// <summary>URL to the campus display image.</summary>
        public string? CampusImage { get; set; }

        /// <summary>Indicates if this record is backed up on Android.</summary>
        public bool IsAndroidBackedUp { get; set; } = false;

        /// <summary>Indicates if this record is backed up on the server.</summary>
        public bool IsServerBackedUp { get; set; } = false;

        /// <summary>The date the campus record was created.</summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>The date the campus record was last updated.</summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>List of buildings belonging to this campus.</summary>
        public ICollection<BuildingResponseDTO>? Buildings { get; set; }
    }
}
