using KovsieAssetsTracker.WebAppMVC.Models.Asset;

namespace KovsieAssetsTracker.WebAppMVC.Models.Room
{
    public class RoomViewModel
    {
        public int RoomServerId { get; set; }
        public int? RoomLocalId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string? RoomDescription { get; set; }
        public string? RoomImage { get; set; }
        public bool IsAndroidBackedUp { get; set; } =false;
        public bool IsServerBackedUp { get; set; } = false;
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public int? FloorServerId { get; set; }
        public string? FloorName { get; set; }

        public List<AssetViewModel>? Assets { get; set; } = new();
    }
}
