using Microsoft.AspNetCore.Http;

namespace KovsieAssetsTracker.Application.DTOs.AssetDTOs
{
    public class UpdateAssetRequestDTO
    {
        public int AssetServerId { get; set; }
        public int? AssetLocalId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string AssetDescription { get; set; } = string.Empty;
        public string? QRCodeSecretText { get; set; }
        public string? Status { get; set; }
        public bool IsAndroidBackedUp { get; set; } =false;
        public bool IsServerBackedUp { get; set; } = false;

        public int? RoomServerId { get; set; }
        public int? AssetCategoryServerId { get; set; }


        public IFormFile? ImageFileAsset { get; set; }
        public IFormFile? ImageFileQrCode { get; set; }
    }
}
