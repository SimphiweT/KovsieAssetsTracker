namespace KovsieAssetsTracker.WebAppMVC.Models.Floor
{
    public class UpdateFloorViewModel
    {
        public int FloorServerId { get; set; }
        public int? FloorLocalId { get; set; }
        public string FloorName { get; set; } = string.Empty;
        public string? FloorDescription { get; set; }
        public bool IsAndroidBackedUp { get; set; } = false;
        public bool IsServerBackedUp { get; set; } = false;
        public int? BuildingServerId { get; set; }
    }
}
