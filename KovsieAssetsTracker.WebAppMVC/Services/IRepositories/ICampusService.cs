using KovsieAssetsTracker.WebAppMVC.Models.Campus;
using KovsieAssetsTracker.WebAppMVC.Models.shared;

namespace KovsieAssetsTracker.WebAppMVC.Services.IRepositories
{
    public interface ICampusService
    {
        Task<ApiResponse<List<CampusViewModel>>> GetAllCampusesAsync();
        Task<ApiResponse<List<CampusViewModel>>> GetAllCampusesWithBuildingsAsync();
        Task<ApiResponse<CampusViewModel>> GetCampusByIdAsync(int id);
        Task<ApiResponse<CampusViewModel>> GetCampusByIdWithBuildingsAsync(int id);
        Task<ApiResponse<CampusViewModel>> CreateCampusAsync(CreateCampusViewModel model);
        Task<ApiResponse<CampusViewModel>> UpdateCampusAsync(UpdateCampusViewModel model);
        Task<ApiResponse<string>> DeleteCampusAsync(int id);
    }
}
