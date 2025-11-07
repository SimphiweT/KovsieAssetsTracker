using KovsieAssetsTracker.WebAppMVC.Models.Asset;

namespace KovsieAssetsTracker.WebAppMVC.Models.Category
{
    public class CategoryViewModel
    {
        public int AssetCategoryServerId { get; set; }
        public int? AssetCategoryLocalId { get; set; }
        public string AssetCategoryName { get; set; } = string.Empty;
        public string? AssetCategoryDescription { get; set; }
        public string? AssetCategoryImage { get; set; }
        public bool IsAndroidBackedUp { get; set; } = false;
        public bool IsServerBackedUp { get; set; } = false;
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Relationship
        public List<AssetViewModel>? Assets { get; set; }
    }
}
