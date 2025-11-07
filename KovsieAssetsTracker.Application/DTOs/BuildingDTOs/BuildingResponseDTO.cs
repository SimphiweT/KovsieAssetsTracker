using KovsieAssetsTracker.Application.DTOs.FloorDTOs;

namespace KovsieAssetsTracker.Application.DTOs.BuildingDTOs
{
    /// <summary>
    /// Represents the data returned when retrieving building information.
    /// </summary>
    public class BuildingResponseDTO
    {
        /// <summary>The unique ID of the building on the server.</summary>
        public int BuildingServerId { get; set; }

        /// <summary>The local (offline) ID used by Android or other clients.</summary>
        public int? BuildingLocalId { get; set; }

        /// <summary>The name of the building.</summary>
        public string BuildingName { get; set; } = string.Empty;

        /// <summary>A brief description of the building’s purpose or type.</summary>
        public string BuildingDescription { get; set; } = string.Empty;

        /// <summary>URL to the building’s display image.</summary>
        public string? BuildingImage { get; set; }

        /// <summary>Indicates whether this record is backed up on Android.</summary>
        public bool IsAndroidBackedUp { get; set; } = false;

        /// <summary>Indicates whether this record is backed up on the server.</summary>
        public bool IsServerBackedUp { get; set; } = false;

        /// <summary>The date the building record was created.</summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>The date the building record was last updated.</summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>Reference to the related campus (if any).</summary>
        public int? CampusServerId { get; set; }

        /// <summary>Name of the related campus.</summary>
        public string? CampusName { get; set; }

        /// <summary>List of floors associated with this building.</summary>
        public ICollection<FloorResponseDTO>? Floors { get; set; }
    }
}
