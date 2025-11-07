using KovsieAssetsTracker.WebAppMVC.Models.Category;
using KovsieAssetsTracker.WebAppMVC.Models.Room;

namespace KovsieAssetsTracker.WebAppMVC.Models.Asset
{
    public class DetailsAndUpdateAssetViewModel
    {
        // Current asset details to display
        public AssetViewModel AssetDetails { get; set; } = new();

        // Data for update form
        public UpdateAssetViewModel UpdateAsset { get; set; } = new();

        // Dropdowns
        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<RoomViewModel> Rooms { get; set; } = new();
    }
}
