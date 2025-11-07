

namespace KovsieAssetsTracker.WebAppMVC.Models.Campus
{
    public class DetailsAndUpdateCampusViewModel
    {
        public CampusViewModel CampusDetails { get; set; } = new();
        public UpdateCampusViewModel UpdateCampus { get; set; } = new();
    }
}
