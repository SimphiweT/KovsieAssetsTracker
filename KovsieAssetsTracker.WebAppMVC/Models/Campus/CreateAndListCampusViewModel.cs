
namespace KovsieAssetsTracker.WebAppMVC.Models.Campus
{
    public class CreateAndListCampusViewModel
    {
        public CreateCampusViewModel CreateCampus { get; set; } = new();
        public List<CampusViewModel> Campuses { get; set; } = new();
    }
}
