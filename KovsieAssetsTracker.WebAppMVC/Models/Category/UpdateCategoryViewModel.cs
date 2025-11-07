namespace KovsieAssetsTracker.WebAppMVC.Models.Category
{
    public class UpdateCategoryViewModel
    {
        public int AssetCategoryServerId { get; set; }
        public int? AssetCategoryLocalId { get; set; }
        public string AssetCategoryName { get; set; } = string.Empty;
        public string? AssetCategoryDescription { get; set; }
        public bool IsAndroidBackedUp { get; set; }= false;
        public bool IsServerBackedUp { get; set; } = false;
        public IFormFile? ImageFile { get; set; }
    }
}
