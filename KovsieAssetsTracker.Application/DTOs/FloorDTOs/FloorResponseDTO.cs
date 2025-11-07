using KovsieAssetsTracker.Application.DTOs.RoomDTOs;

namespace KovsieAssetsTracker.Application.DTOs.FloorDTOs
{
    /// <summary>
    /// Represents the data returned when retrieving floor information.
    /// </summary>
    public class FloorResponseDTO
    {
        /// <summary>The unique server ID of the floor.</summary>
        public int FloorServerId { get; set; }

        /// <summary>The local (offline) ID used by Android clients.</summary>
        public int? FloorLocalId { get; set; }

        /// <summary>The name of the floor.</summary>
        public string FloorName { get; set; } = string.Empty;

        /// <summary>A brief description of the floor.</summary>
        public string? FloorDescription { get; set; }

        /// <summary>Indicates if this record is backed up on Android.</summary>
        public bool IsAndroidBackedUp { get; set; } = false;

        /// <summary>Indicates if this record is backed up on the server.</summary>
        public bool IsServerBackedUp { get; set; } = false;

        /// <summary>The date when this record was created.</summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>The date when this record was last updated.</summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>The ID of the building this floor belongs to.</summary>
        public int? BuildingServerId { get; set; }

        /// <summary>The name of the building this floor belongs to.</summary>
        public string? BuildingName { get; set; }

        /// <summary>List of rooms associated with this floor.</summary>
        public ICollection<RoomResponseDTO>? Rooms { get; set; }
    }
}
