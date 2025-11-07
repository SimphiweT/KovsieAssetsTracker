using KovsieAssetsTracker.WebAppMVC.Models.Category;
using KovsieAssetsTracker.WebAppMVC.Models.shared;

namespace KovsieAssetsTracker.WebAppMVC.Services.IRepositories
{
    public interface ICategoryService
    {
        Task<ApiResponse<List<CategoryViewModel>>> GetAllCategoriesAsync();
        Task<ApiResponse<List<CategoryViewModel>>> GetAllCategoriesWithAssetsAsync();
        Task<ApiResponse<CategoryViewModel>> GetCategoryByIdAsync(int id);
        Task<ApiResponse<CategoryViewModel>> GetCategoryByIdWithAssetsAsync(int id);
        Task<ApiResponse<CategoryViewModel>> CreateCategoryAsync(CreateCategoryViewModel model);
        Task<ApiResponse<CategoryViewModel>> UpdateCategoryAsync(UpdateCategoryViewModel model);
        Task<ApiResponse<string>> DeleteCategoryAsync(int id);
    }
}
