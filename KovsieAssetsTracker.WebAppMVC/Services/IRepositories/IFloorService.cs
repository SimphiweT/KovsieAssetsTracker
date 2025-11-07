using KovsieAssetsTracker.WebAppMVC.Models.Floor;
using KovsieAssetsTracker.WebAppMVC.Models.shared;

namespace KovsieAssetsTracker.WebAppMVC.Services.IRepositories
{
    public interface IFloorService
    {
        Task<ApiResponse<List<FloorViewModel>>> GetAllFloorsAsync();
        Task<ApiResponse<List<FloorViewModel>>> GetAllFloorsWithRoomsAsync();
        Task<ApiResponse<FloorViewModel>> GetFloorByIdAsync(int id);
        Task<ApiResponse<FloorViewModel>> GetFloorByIdWithRoomsAsync(int id);
        Task<ApiResponse<FloorViewModel>> CreateFloorAsync(CreateFloorViewModel model);
        Task<ApiResponse<FloorViewModel>> UpdateFloorAsync(UpdateFloorViewModel model);
        Task<ApiResponse<string>> DeleteFloorAsync(int id);
    }
}
