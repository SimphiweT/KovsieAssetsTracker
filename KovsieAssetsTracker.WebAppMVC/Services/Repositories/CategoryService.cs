using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Category;
using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace KovsieAssetsTracker.WebAppMVC.Services.Repositories
{
    public class CategoryService: ICategoryService
    {
        private readonly HttpClient client;
        private readonly string apiBaseUrl;

        public CategoryService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
        {
            client = _client;
            apiBaseUrl = $"{_apiSettings.Value.BaseUrl}assetcategories";
        }

        /* ============================= GET ALL ============================= */
        public async Task<ApiResponse<List<CategoryViewModel>>> GetAllCategoriesAsync()
        {
            return await GetAsync<ApiResponse<List<CategoryViewModel>>>(apiBaseUrl);
        }

        public async Task<ApiResponse<List<CategoryViewModel>>> GetAllCategoriesWithAssetsAsync()
        {
            return await GetAsync<ApiResponse<List<CategoryViewModel>>>($"{apiBaseUrl}/get_all_categories_with_assets");
        }

        /* ============================= GET BY ID ============================= */
        public async Task<ApiResponse<CategoryViewModel>> GetCategoryByIdAsync(int id)
        {
            return await GetAsync<ApiResponse<CategoryViewModel>>($"{apiBaseUrl}/{id}");
        }

        public async Task<ApiResponse<CategoryViewModel>> GetCategoryByIdWithAssetsAsync(int id)
        {
            return await GetAsync<ApiResponse<CategoryViewModel>>($"{apiBaseUrl}/get_category_with_assets/{id}");
        }

        /* ============================= CREATE ============================= */
        public async Task<ApiResponse<CategoryViewModel>> CreateCategoryAsync(CreateCategoryViewModel model)
        {
            try
            {
                using var form = new MultipartFormDataContent
                {
                    { new StringContent(model.AssetCategoryLocalId?.ToString() ?? ""), "AssetCategoryLocalId" },
                    { new StringContent(model.AssetCategoryName ?? ""), "AssetCategoryName" },
                    { new StringContent(model.AssetCategoryDescription ?? ""), "AssetCategoryDescription" },
                    { new StringContent(model.IsAndroidBackedUp.ToString()), "IsAndroidBackedUp" },
                    { new StringContent(model.IsServerBackedUp.ToString()), "IsServerBackedUp" }
                };

                if (model.ImageFile != null)
                {
                    var fileContent = new StreamContent(model.ImageFile.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                    form.Add(fileContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await client.PostAsync(apiBaseUrl, form);

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[CreateCategory] Failed: {response.StatusCode} → {json}");
                }

                return await DeserializeResponse<ApiResponse<CategoryViewModel>>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<CategoryViewModel> { Success = false, Error = ex.Message };
            }
        }

        /* ============================= UPDATE ============================= */
        public async Task<ApiResponse<CategoryViewModel>> UpdateCategoryAsync(UpdateCategoryViewModel model)
        {
            try
            {
                using var form = new MultipartFormDataContent
                {
                    { new StringContent(model.AssetCategoryServerId.ToString()), "AssetCategoryServerId" },
                    { new StringContent(model.AssetCategoryLocalId?.ToString() ?? ""), "AssetCategoryLocalId" },
                    { new StringContent(model.AssetCategoryName ?? ""), "AssetCategoryName" },
                    { new StringContent(model.AssetCategoryDescription ?? ""), "AssetCategoryDescription" },
                    { new StringContent(model.IsAndroidBackedUp.ToString()), "IsAndroidBackedUp" },
                    { new StringContent(model.IsServerBackedUp.ToString()), "IsServerBackedUp" }
                };

                if (model.ImageFile != null)
                {
                    var fileContent = new StreamContent(model.ImageFile.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                    form.Add(fileContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await client.PutAsync(apiBaseUrl, form);
                return await DeserializeResponse<ApiResponse<CategoryViewModel>>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<CategoryViewModel> { Success = false, Error = ex.Message };
            }
        }

        /* ============================= DELETE ============================= */
        public async Task<ApiResponse<string>> DeleteCategoryAsync(int id)
        {
            try
            {
                var response = await client.DeleteAsync($"{apiBaseUrl}/{id}");
                return await DeserializeResponse<ApiResponse<string>>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, Error = ex.Message };
            }
        }

        /* ============================= HELPERS ============================= */
        private async Task<T> GetAsync<T>(string url)
        {
            var response = await client.GetAsync(url);
            return await DeserializeResponse<T>(response);
        }

        private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json)!;
        }
    }
}
