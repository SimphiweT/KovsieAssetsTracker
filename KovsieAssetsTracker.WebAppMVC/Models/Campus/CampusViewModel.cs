using KovsieAssetsTracker.WebAppMVC.Models.Building;

namespace KovsieAssetsTracker.WebAppMVC.Models.Campus
{
    public class CampusViewModel
    {
        public int CampusServerId { get; set; }
        public int? CampusLocalId { get; set; }
        public string CampusName { get; set; } = string.Empty;
        public string CampusDescription { get; set; } = string.Empty;
        public string? CampusImage { get; set; }
        public bool IsAndroidBackedUp { get; set; } = false;
        public bool IsServerBackedUp { get; set; } = false;
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Relationships
        public List<BuildingViewModel>? Buildings { get; set; }
    }
}
