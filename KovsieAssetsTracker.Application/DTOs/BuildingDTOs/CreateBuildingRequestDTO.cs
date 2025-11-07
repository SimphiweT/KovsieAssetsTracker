using Microsoft.AspNetCore.Http;

namespace KovsieAssetsTracker.Application.DTOs.BuildingDTOs
{
    /// <summary>
    /// Represents the data required to create a new building.
    /// </summary>
    public class CreateBuildingRequestDTO
    {
        /// <summary>The local (offline) ID used by Android clients.</summary>
        public int? BuildingLocalId { get; set; }

        /// <summary>The name of the building to create.</summary>
        public string BuildingName { get; set; } = string.Empty;

        /// <summary>A short description of the building.</summary>
        public string BuildingDescription { get; set; } = string.Empty;

        /// <summary>Whether this record is backed up on Android.</summary>
        public bool IsAndroidBackedUp { get; set; } = false;

        /// <summary>Whether this record is backed up on the server.</summary>
        public bool IsServerBackedUp { get; set; } = false;

        /// <summary>The campus ID this building belongs to.</summary>
        public int? CampusServerId { get; set; }

        /// <summary>The uploaded image file representing this building.</summary>
        public IFormFile? ImageFile { get; set; }
    }
}
