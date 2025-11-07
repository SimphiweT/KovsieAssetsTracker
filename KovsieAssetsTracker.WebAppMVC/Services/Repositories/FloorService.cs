using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Floor;
using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace KovsieAssetsTracker.WebAppMVC.Services.Repositories
{
    public class FloorService : IFloorService
    {
        private readonly HttpClient client;
        private readonly string apiBaseUrl;

        public FloorService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
        {
            client = _client;
            apiBaseUrl = $"{_apiSettings.Value.BaseUrl}floors";
        }

        /* ============================= GET ALL ============================= */
        public async Task<ApiResponse<List<FloorViewModel>>> GetAllFloorsAsync()
        {
            return await GetAsync<ApiResponse<List<FloorViewModel>>>(apiBaseUrl);
        }

        public async Task<ApiResponse<List<FloorViewModel>>> GetAllFloorsWithRoomsAsync()
        {
            return await GetAsync<ApiResponse<List<FloorViewModel>>>($"{apiBaseUrl}/get_all_floors_with_rooms");
        }

        /* ============================= GET BY ID ============================= */
        public async Task<ApiResponse<FloorViewModel>> GetFloorByIdAsync(int id)
        {
            return await GetAsync<ApiResponse<FloorViewModel>>($"{apiBaseUrl}/{id}");
        }

        public async Task<ApiResponse<FloorViewModel>> GetFloorByIdWithRoomsAsync(int id)
        {
            return await GetAsync<ApiResponse<FloorViewModel>>($"{apiBaseUrl}/get_floor_with_rooms/{id}");
        }

        /* ============================= CREATE ============================= */
        public async Task<ApiResponse<FloorViewModel>> CreateFloorAsync(CreateFloorViewModel model)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiBaseUrl, content);
                return await DeserializeResponse<ApiResponse<FloorViewModel>>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<FloorViewModel> { Success = false, Error = ex.Message };
            }
        }

        /* ============================= UPDATE ============================= */
        public async Task<ApiResponse<FloorViewModel>> UpdateFloorAsync(UpdateFloorViewModel model)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(apiBaseUrl, content);
                return await DeserializeResponse<ApiResponse<FloorViewModel>>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<FloorViewModel> { Success = false, Error = ex.Message };
            }
        }

        /* ============================= DELETE ============================= */
        public async Task<ApiResponse<string>> DeleteFloorAsync(int id)
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
