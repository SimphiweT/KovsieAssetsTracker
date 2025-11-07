using KovsieAssetsTracker.WebAppMVC.Models.Building;
using KovsieAssetsTracker.WebAppMVC.Models.shared;

namespace KovsieAssetsTracker.WebAppMVC.Services.IRepositories
{
    public interface IBuildingService
    {
        Task<ApiResponse<List<BuildingViewModel>>> GetAllBuildingsAsync();
        Task<ApiResponse<List<BuildingViewModel>>> GetAllBuildingsWithFloorsAsync();
        Task<ApiResponse<BuildingViewModel>> GetBuildingByIdAsync(int id);
        Task<ApiResponse<BuildingViewModel>> GetBuildingByIdWithFloorsAsync(int id);
        Task<ApiResponse<BuildingViewModel>> CreateBuildingAsync(CreateBuildingViewModel model);
        Task<ApiResponse<BuildingViewModel>> UpdateBuildingAsync(UpdateBuildingViewModel model);
        Task<ApiResponse<string>> DeleteBuildingAsync(int id);
    }
}
