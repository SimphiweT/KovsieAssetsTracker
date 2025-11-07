
using KovsieAssetsTracker.WebAppMVC.Models.Campus;
using KovsieAssetsTracker.WebAppMVC.Models.Room;

namespace KovsieAssetsTracker.WebAppMVC.Models.Building
{
    public class CreateAndListBuildingsViewModel
    {
        public CreateBuildingViewModel CreateBuilding { get; set; } = new();
        public List<BuildingViewModel> Buildings { get; set; } = new();
        public List<RoomViewModel> Rooms { get; set; } = new();
        public List<CampusViewModel>? Campuses { get; set; }
    }
}
