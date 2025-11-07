using Microsoft.AspNetCore.Http;

namespace KovsieAssetsTracker.Application.DTOs.BuildingDTOs
{
    /// <summary>
    /// Represents the data required to update an existing building.
    /// </summary>
    public class UpdateBuildingRequestDTO
    {
        /// <summary>The unique server ID of the building to update.</summary>
        public int BuildingServerId { get; set; }

        /// <summary>The local (offline) ID used by Android clients.</summary>
        public int? BuildingLocalId { get; set; }

        /// <summary>The name of the building.</summary>
        public string BuildingName { get; set; } = string.Empty;

        /// <summary>The building’s description.</summary>
        public string BuildingDescription { get; set; } = string.Empty;

        /// <summary>Indicates whether this record is backed up on Android.</summary>
        public bool IsAndroidBackedUp { get; set; } = false;

        /// <summary>Indicates whether this record is backed up on the server.</summary>
        public bool IsServerBackedUp { get; set; } = false;

        /// <summary>The campus this building belongs to (nullable).</summary>
        public int? CampusServerId { get; set; }

        /// <summary>Optional new image for this building.</summary>
        public IFormFile? ImageFile { get; set; }
    }
}
