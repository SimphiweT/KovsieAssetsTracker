using KovsieAssetsTracker.WebAppMVC.Models.Room;
using KovsieAssetsTracker.WebAppMVC.Models.shared;

namespace KovsieAssetsTracker.WebAppMVC.Services.IRepositories
{
    public interface IRoomService
    {
        Task<ApiResponse<List<RoomViewModel>>> GetAllRoomsAsync();
        Task<ApiResponse<RoomViewModel>> GetRoomByIdAsync(int id);
        //Task<ApiResponse<RoomViewModel>> GetRoomByIdWithAssetsAsync(int id);
        //Task<ApiResponse<List<RoomViewModel>>> GetAllRoomsWithAssetsAsync();
        Task<ApiResponse<RoomViewModel>> CreateRoomAsync(CreateRoomViewModel room);
        Task<ApiResponse<RoomViewModel>> UpdateRoomAsync(UpdateRoomViewModel room);
        Task<ApiResponse<string>> DeleteRoomAsync(int id);
    }
}
