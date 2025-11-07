using KovsieAssetsTracker.WebAppMVC.Models.Category;
using KovsieAssetsTracker.WebAppMVC.Models.Room;

namespace KovsieAssetsTracker.WebAppMVC.Models.Asset
{
    public class UpdateAndListAssetViewModel
    {
        // Create form
        public UpdateAssetViewModel UpdateAsset { get; set; } = new();

        // List of existing assets
        public List<AssetViewModel> Assets { get; set; } = new();

        // Dropdown lists
        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<RoomViewModel> Rooms { get; set; } = new();
    }
}
