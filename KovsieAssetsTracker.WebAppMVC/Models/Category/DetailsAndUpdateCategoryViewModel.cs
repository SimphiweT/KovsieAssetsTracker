namespace KovsieAssetsTracker.WebAppMVC.Models.Category
{
    public class DetailsAndUpdateCategoryViewModel
    {
        public CategoryViewModel CategoryDetails { get; set; } = new();
        public UpdateCategoryViewModel UpdateCategory { get; set; } = new();
    }
}
