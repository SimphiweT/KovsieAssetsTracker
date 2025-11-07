namespace KovsieAssetsTracker.WebAppMVC.Models.Building
{
    public class CreateBuildingViewModel
    {
        public int? BuildingLocalId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string BuildingDescription { get; set; } = string.Empty;
        public bool IsAndroidBackedUp { get; set; } = false;
        public bool IsServerBackedUp { get; set; } = false;
        public int? CampusServerId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
