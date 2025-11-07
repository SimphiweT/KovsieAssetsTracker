namespace KovsieAssetsTracker.WebAppMVC.Models.Category
{
    public class CreateAndListCategoryViewModel
    {
        public CreateCategoryViewModel CreateCategory { get; set; } =new();
        public List<CategoryViewModel> Categories { get; set; } =new();
    }
}
