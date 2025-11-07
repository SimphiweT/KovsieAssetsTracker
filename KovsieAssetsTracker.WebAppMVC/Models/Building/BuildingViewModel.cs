using KovsieAssetsTracker.WebAppMVC.Models.Floor;

namespace KovsieAssetsTracker.WebAppMVC.Models.Building
{
    public class BuildingViewModel
    {
        public int BuildingServerId { get; set; }
        public int? BuildingLocalId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string BuildingDescription { get; set; } = string.Empty;
        public string? BuildingImage { get; set; }
        public bool IsAndroidBackedUp { get; set; } =false;
        public bool IsServerBackedUp { get; set; } = false;
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public int? CampusServerId { get; set; }
        public string? CampusName { get; set; }

        public List<FloorViewModel>? Floors { get; set; }
    }
}
