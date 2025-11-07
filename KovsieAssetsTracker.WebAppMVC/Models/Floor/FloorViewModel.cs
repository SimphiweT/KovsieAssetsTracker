using KovsieAssetsTracker.WebAppMVC.Models.Room;

namespace KovsieAssetsTracker.WebAppMVC.Models.Floor
{
    public class FloorViewModel
    {
        public int FloorServerId { get; set; }
        public int? FloorLocalId { get; set; }
        public string FloorName { get; set; } = string.Empty;
        public string? FloorDescription { get; set; }
        public bool IsAndroidBackedUp { get; set; } = false;
        public bool IsServerBackedUp { get; set; }=false;
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public int? BuildingServerId { get; set; }
        public string? BuildingName { get; set; }

        public List<RoomViewModel>? Rooms { get; set; }
    }
}
