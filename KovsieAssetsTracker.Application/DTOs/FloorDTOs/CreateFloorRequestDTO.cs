namespace KovsieAssetsTracker.Application.DTOs.FloorDTOs
{
    /// <summary>
    /// Represents the data required to create a new floor.
    /// </summary>
    public class CreateFloorRequestDTO
    {
        /// <summary>The local (offline) ID used by Android clients.</summary>
        public int? FloorLocalId { get; set; }

        /// <summary>The name of the floor to create.</summary>
        public string FloorName { get; set; } = string.Empty;

        /// <summary>An optional description of the floor.</summary>
        public string? FloorDescription { get; set; }

        /// <summary>Indicates if this record is backed up on Android.</summary>
        public bool IsAndroidBackedUp { get; set; } = false;

        /// <summary>Indicates if this record is backed up on the server.</summary>
        public bool IsServerBackedUp { get; set; } = false;

        /// <summary>The ID of the building this floor belongs to.</summary>
        public int? BuildingServerId { get; set; }
    }
}
