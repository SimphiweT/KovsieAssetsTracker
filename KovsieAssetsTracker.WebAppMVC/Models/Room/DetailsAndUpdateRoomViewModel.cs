
using KovsieAssetsTracker.WebAppMVC.Models.Floor;

namespace KovsieAssetsTracker.WebAppMVC.Models.Room
{
    public class DetailsAndUpdateRoomViewModel
    {
        public RoomViewModel RoomDetails { get; set; } = new();
        public UpdateRoomViewModel UpdateRoom { get; set; } = new();
        public List<FloorViewModel> Floors { get; set; } = new();
    }
}
