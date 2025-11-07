namespace KovsieAssetsTracker.WebAppMVC.Models.Room
{
    public class UpdateRoomViewModel
    {
        public int RoomServerId { get; set; }
        public int? RoomLocalId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string? RoomDescription { get; set; }
        public bool IsAndroidBackedUp { get; set; } = false;
        public bool IsServerBackedUp { get; set; } = false;
        public int? FloorServerId { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
