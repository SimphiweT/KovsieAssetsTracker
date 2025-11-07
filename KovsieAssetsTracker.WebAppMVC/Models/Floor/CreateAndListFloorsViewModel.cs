
using KovsieAssetsTracker.WebAppMVC.Models.Building;
using KovsieAssetsTracker.WebAppMVC.Models.Room;

namespace KovsieAssetsTracker.WebAppMVC.Models.Floor
{
    public class CreateAndListFloorsViewModel
    {
        public CreateFloorViewModel CreateFloor { get; set; } = new();
        public List<FloorViewModel> Floors { get; set; } = new();
        public List<RoomViewModel> Rooms { get; set; } = new();
        public List<BuildingViewModel> Buildings { get; set; } = new();
    }
}
