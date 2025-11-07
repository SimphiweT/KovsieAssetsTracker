using KovsieAssetsTracker.WebAppMVC.Models.Asset;
using KovsieAssetsTracker.WebAppMVC.Models.Room;

namespace KovsieAssetsTracker.WebAppMVC.Models.Staff
{
    public class StaffDashboardViewModel
    {
        // Totals
        public int TotalAssets { get; set; }
        public int TotalCategories { get; set; }
        public int TotalRooms { get; set; }

        // Charts
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryCounts { get; set; } = new();
        public List<string> RoomLabels { get; set; } = new();
        public List<int> RoomCounts { get; set; } = new();

        // Recent
        public List<AssetViewModel> RecentAssets { get; set; } = new();
        public List<RoomViewModel> RecentRooms { get; set; } = new();

        public RoomViewModel RoomViewModel { get; set; } = new();
        public AssetViewModel AssetViewModel { get; set; } = new();
    }
}
