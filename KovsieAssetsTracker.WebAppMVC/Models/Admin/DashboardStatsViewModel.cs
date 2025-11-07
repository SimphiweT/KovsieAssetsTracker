namespace KovsieAssetsTracker.WebAppMVC.Models.Admin
{
    public class DashboardStatsViewModel
    {
        public int TotalAssets { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRooms { get; set; }

        // Top 5 Assets
        public List<string> TopAssetNames { get; set; } = new();
        public List<int> TopAssetCounts { get; set; } = new();

        // Assets per Room
        public List<string> RoomLabels { get; set; } = new();
        public List<int> RoomAssetCounts { get; set; } = new();

        // Assets by Category
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryCounts { get; set; } = new();

        // Assets by Status
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();
    }
}
