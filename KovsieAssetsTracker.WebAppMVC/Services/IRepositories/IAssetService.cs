using KovsieAssetsTracker.WebAppMVC.Models.Asset;
using KovsieAssetsTracker.WebAppMVC.Models.shared;

namespace KovsieAssetsTracker.WebAppMVC.Services.IRepositories
{
    public interface IAssetService
    {
        Task<ApiResponse<List<AssetViewModel>>> GetAllAssetsAsync();
        Task<ApiResponse<AssetViewModel>> GetAssetByIdAsync(int id);
        Task<ApiResponse<AssetViewModel>> GetAssetByQrCodeAsync(string qrCode);
        Task<ApiResponse<AssetViewModel>> CreateAssetAsync(CreateAssetViewModel newAsset);
        Task<ApiResponse<AssetViewModel>> UpdateAssetAsync(UpdateAssetViewModel asset);
        Task<ApiResponse<string>> DeleteAssetAsync(int id);
    }
}
