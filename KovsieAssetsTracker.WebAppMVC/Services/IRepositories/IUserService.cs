using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Models.User;

namespace KovsieAssetsTracker.WebAppMVC.Services.IRepositories
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserViewModel>>> GetAllUsersAsync();
        Task<ApiResponse<UserViewModel>> GetUserByIdAsync(string userId);
        Task<ApiResponse<UserViewModel>> RegisterUserAsync(CreateUserViewModel newUser);
        Task<ApiResponse<UserViewModel>> UpdateUserAsync(UpdateUserViewModel newUser);
        Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordViewModel changePassword);
        Task<ApiResponse<string>> AdminResetPasswordAsync(string email, string temporaryPassword);
        Task<ApiResponse<string>> DeleteUserAsync(string userId);
    }
}
