namespace KovsieAssetsTracker.WebAppMVC.Models.Asset
{
    public class AssetViewModel
    {
        public int AssetServerId { get; set; }
        public int? AssetLocalId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string AssetDescription { get; set; } = string.Empty;
        public string? AssetImage { get; set; }
        public string? AssetQRCodeImage { get; set; }
        public string? QRCodeSecretText { get; set; }
        public string? Status { get; set; }
        public bool IsAndroidBackedUp { get; set; } = false;
        public bool IsServerBackedUp { get; set; } =false;
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public int? RoomServerId { get; set; }
        public string? RoomName { get; set; }

        public int? AssetCategoryServerId { get; set; }
        public string? AssetCategoryName { get; set; }
    }
}
