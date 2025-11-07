namespace KovsieAssetsTracker.WebAppMVC.Models.Campus
{
    public class CreateCampusViewModel
    {
        public int? CampusLocalId { get; set; }
        public string CampusName { get; set; } = string.Empty;
        public string CampusDescription { get; set; } = string.Empty;
        public bool IsAndroidBackedUp { get; set; } = false;
        public bool IsServerBackedUp { get; set; } = false;
        public IFormFile? ImageFile { get; set; }
    }
}
