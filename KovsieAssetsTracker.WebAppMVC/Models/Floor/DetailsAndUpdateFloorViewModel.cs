using KovsieAssetsTracker.WebAppMVC.Models.Building;
using KovsieAssetsTracker.WebAppMVC.Models.Room;

namespace KovsieAssetsTracker.WebAppMVC.Models.Floor
{
    public class DetailsAndUpdateFloorViewModel
    {
        public FloorViewModel FloorDetails { get; set; } = new();
        public UpdateFloorViewModel UpdateFloor { get; set; } = new();
        public List<RoomViewModel> Rooms { get; set; } = new();
        public List<BuildingViewModel> Buildings { get; set; } = new();
    }
}
