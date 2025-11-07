
using KovsieAssetsTracker.WebAppMVC.Models.Campus;
using KovsieAssetsTracker.WebAppMVC.Models.Room;

namespace KovsieAssetsTracker.WebAppMVC.Models.Building
{
    public class DetailsAndUpdateBuildingViewModel
    {
        public BuildingViewModel BuildingDetails { get; set; } = new();
        public UpdateBuildingViewModel UpdateBuilding { get; set; } = new();
        public List<RoomViewModel> Rooms { get; set; } = new();
        public List<CampusViewModel>? Campuses { get; set; }
    }
}
