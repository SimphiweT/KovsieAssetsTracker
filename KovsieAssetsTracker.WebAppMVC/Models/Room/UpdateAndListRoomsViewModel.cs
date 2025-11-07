using KovsieAssetsTracker.WebAppMVC.Models.Floor;

namespace KovsieAssetsTracker.WebAppMVC.Models.Room
{
    public class UpdateAndListRoomsViewModel
    {
        public UpdateRoomViewModel UpdateRoom { get; set; } = new();
        public List<RoomViewModel> Rooms { get; set; } = new();
        public List<FloorViewModel> Floors { get; set; } = new();
    }
}
