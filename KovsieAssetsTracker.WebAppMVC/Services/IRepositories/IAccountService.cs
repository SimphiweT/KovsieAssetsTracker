using KovsieAssetsTracker.WebAppMVC.Models.Account;
using KovsieAssetsTracker.WebAppMVC.Models.shared;

namespace KovsieAssetsTracker.WebAppMVC.Services.IRepositories
{
    public interface IAccountService
    {
        Task<ApiResponse<LoginResponseViewModel>> LoginAsync(LoginViewModel login);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordViewModel resetPassword);
        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordViewModel forgotPassword);
        Task<ApiResponse<string>> RequestAccessAsync(SignUpViewModel signUp);
        Task<ApiResponse<List<AccessResponseViewModel>>> GetAllAccessRequestsAsync();
        Task<ApiResponse<string>> ApproveAccessRequestAsync(int id);
        Task<ApiResponse<string>> DeclineAccessRequestAsync(int id, string reason);
        Task<string> LogoutAsync();
    }
}
