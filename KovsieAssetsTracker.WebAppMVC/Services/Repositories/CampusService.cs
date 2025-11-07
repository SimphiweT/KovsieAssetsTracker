using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Campus;
using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace KovsieAssetsTracker.WebAppMVC.Services.Repositories
{
    public class CampusService: ICampusService
    {
        private readonly HttpClient client;
        private readonly string apiBaseUrl;

        public CampusService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
        {
            client = _client;
            apiBaseUrl = $"{_apiSettings.Value.BaseUrl}campuses";
        }

        /* ============================= GET ALL ============================= */
        public async Task<ApiResponse<List<CampusViewModel>>> GetAllCampusesAsync()
        {
            return await GetAsync<ApiResponse<List<CampusViewModel>>>(apiBaseUrl);
        }

        public async Task<ApiResponse<List<CampusViewModel>>> GetAllCampusesWithBuildingsAsync()
        {
            return await GetAsync<ApiResponse<List<CampusViewModel>>>($"{apiBaseUrl}/get_all_campuses_with_buildings");
        }

        /* ============================= GET BY ID ============================= */
        public async Task<ApiResponse<CampusViewModel>> GetCampusByIdAsync(int id)
        {
            return await GetAsync<ApiResponse<CampusViewModel>>($"{apiBaseUrl}/{id}");
        }

        public async Task<ApiResponse<CampusViewModel>> GetCampusByIdWithBuildingsAsync(int id)
        {
            return await GetAsync<ApiResponse<CampusViewModel>>($"{apiBaseUrl}/get_campus_with_buildings/{id}");
        }

        /* ============================= CREATE ============================= */
        public async Task<ApiResponse<CampusViewModel>> CreateCampusAsync(CreateCampusViewModel model)
        {
            try
            {
                using var form = new MultipartFormDataContent
                {
                    { new StringContent(model.CampusLocalId?.ToString() ?? ""), "CampusLocalId" },
                    { new StringContent(model.CampusName ?? ""), "CampusName" },
                    { new StringContent(model.CampusDescription ?? ""), "CampusDescription" },
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
                var result = await DeserializeResponse<ApiResponse<CampusViewModel>>(response);
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponse<CampusViewModel> { Success = false, Error = ex.Message };
            }
        }

        /* ============================= UPDATE ============================= */
        public async Task<ApiResponse<CampusViewModel>> UpdateCampusAsync(UpdateCampusViewModel model)
        {
            try
            {
                using var form = new MultipartFormDataContent
                {
                    { new StringContent(model.CampusServerId.ToString()), "CampusServerId" },
                    { new StringContent(model.CampusLocalId?.ToString() ?? ""), "CampusLocalId" },
                    { new StringContent(model.CampusName ?? ""), "CampusName" },
                    { new StringContent(model.CampusDescription ?? ""), "CampusDescription" },
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
                var result = await DeserializeResponse<ApiResponse<CampusViewModel>>(response);
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponse<CampusViewModel> { Success = false, Error = ex.Message };
            }
        }

        /* ============================= DELETE ============================= */
        public async Task<ApiResponse<string>> DeleteCampusAsync(int id)
        {
            try
            {
                var response = await client.DeleteAsync($"{apiBaseUrl}/{id}");
                var result = await DeserializeResponse<ApiResponse<string>>(response);
                return result;
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
