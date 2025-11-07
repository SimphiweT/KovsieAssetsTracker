namespace KovsieAssetsTracker.WebAppMVC.Models.Category
{
    public class CreateCategoryViewModel
    {
        public int? AssetCategoryLocalId { get; set; }
        public string AssetCategoryName { get; set; } = string.Empty;
        public string AssetCategoryDescription { get; set; } = string.Empty;
        public bool IsAndroidBackedUp { get; set; }=false;
        public bool IsServerBackedUp { get; set; } = false;
        public IFormFile? ImageFile { get; set; }
    }
}
