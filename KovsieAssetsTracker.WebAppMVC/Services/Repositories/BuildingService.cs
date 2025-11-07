using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Building;
using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace KovsieAssetsTracker.WebAppMVC.Services.Repositories
{
    public class BuildingService: IBuildingService
    {
        private readonly HttpClient client;
        private readonly string apiBaseUrl;

        public BuildingService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
        {
            client = _client;
            apiBaseUrl = $"{_apiSettings.Value.BaseUrl}buildings";
        }

        /* ============================= GET ALL ============================= */
        public async Task<ApiResponse<List<BuildingViewModel>>> GetAllBuildingsAsync()
        {
            return await GetAsync<ApiResponse<List<BuildingViewModel>>>(apiBaseUrl);
        }

        public async Task<ApiResponse<List<BuildingViewModel>>> GetAllBuildingsWithFloorsAsync()
        {
            return await GetAsync<ApiResponse<List<BuildingViewModel>>>($"{apiBaseUrl}/get_all_buildings_with_floors");
        }

        /* ============================= GET BY ID ============================= */
        public async Task<ApiResponse<BuildingViewModel>> GetBuildingByIdAsync(int id)
        {
            return await GetAsync<ApiResponse<BuildingViewModel>>($"{apiBaseUrl}/{id}");
        }

        public async Task<ApiResponse<BuildingViewModel>> GetBuildingByIdWithFloorsAsync(int id)
        {
            return await GetAsync<ApiResponse<BuildingViewModel>>($"{apiBaseUrl}/get_building_with_floors/{id}");
        }

        /* ============================= CREATE ============================= */
        public async Task<ApiResponse<BuildingViewModel>> CreateBuildingAsync(CreateBuildingViewModel model)
        {
            try
            {
                using var form = new MultipartFormDataContent
                {
                    { new StringContent(model.BuildingLocalId?.ToString() ?? ""), "BuildingLocalId" },
                    { new StringContent(model.BuildingName ?? ""), "BuildingName" },
                    { new StringContent(model.BuildingDescription ?? ""), "BuildingDescription" },
                    { new StringContent(model.IsAndroidBackedUp.ToString()), "IsAndroidBackedUp" },
                    { new StringContent(model.IsServerBackedUp.ToString()), "IsServerBackedUp" },
                    { new StringContent(model.CampusServerId?.ToString() ?? ""), "CampusServerId" }
                };

                if (model.ImageFile != null)
                {
                    var fileContent = new StreamContent(model.ImageFile.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                    form.Add(fileContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await client.PostAsync(apiBaseUrl, form);
                return await DeserializeResponse<ApiResponse<BuildingViewModel>>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<BuildingViewModel> { Success = false, Error = ex.Message };
            }
        }

        /* ============================= UPDATE ============================= */
        public async Task<ApiResponse<BuildingViewModel>> UpdateBuildingAsync(UpdateBuildingViewModel model)
        {
            try
            {
                using var form = new MultipartFormDataContent
                {
                    { new StringContent(model.BuildingServerId.ToString()), "BuildingServerId" },
                    { new StringContent(model.BuildingLocalId?.ToString() ?? ""), "BuildingLocalId" },
                    { new StringContent(model.BuildingName ?? ""), "BuildingName" },
                    { new StringContent(model.BuildingDescription ?? ""), "BuildingDescription" },
                    { new StringContent(model.IsAndroidBackedUp.ToString()), "IsAndroidBackedUp" },
                    { new StringContent(model.IsServerBackedUp.ToString()), "IsServerBackedUp" },
                    { new StringContent(model.CampusServerId?.ToString() ?? ""), "CampusServerId" }
                };

                if (model.ImageFile != null)
                {
                    var fileContent = new StreamContent(model.ImageFile.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                    form.Add(fileContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await client.PutAsync(apiBaseUrl, form);
                return await DeserializeResponse<ApiResponse<BuildingViewModel>>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<BuildingViewModel> { Success = false, Error = ex.Message };
            }
        }

        /* ============================= DELETE ============================= */
        public async Task<ApiResponse<string>> DeleteBuildingAsync(int id)
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
