namespace KovsieAssetsTracker.WebAppMVC.Models.Category
{
    public class UpdateAndListCategoryViewModel
    {
        public UpdateCategoryViewModel UpdateCategory { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}
