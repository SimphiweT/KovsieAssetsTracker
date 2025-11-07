using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Room;
using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace KovsieAssetsTracker.WebAppMVC.Services.Repositories
{
    public class RoomService : IRoomService
    {
        private HttpClient client;
        private readonly ApiSettings apiSettings;
        private readonly SessionManager sessionManager;
        private readonly string apiBaseUrl;
        public RoomService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
        {
            client = _client;
            apiSettings = _apiSettings.Value;
            sessionManager = _sessionManager;
            apiBaseUrl = $"{apiSettings.BaseUrl}";

            /* setup session manually if not using AuthHeaderHandler
            //Authentication header with Bearer token
            var token = sessionManager.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionManager.GetToken());
            }
            */
        }
        public async Task<ApiResponse<List<RoomViewModel>>> GetAllRoomsAsync()
        {
                var response = await client.GetAsync($"{apiBaseUrl}rooms");
                return await DeserializeResponse<List<RoomViewModel>>(response);
        }

        //public async Task<ApiResponse<List<RoomViewModel>>> GetAllRoomsWithAssetsAsync()
        //{
        //        var response = await client.GetAsync($"{apiBaseUrl}rooms/get_all_rooms_with_assets");
        //        return await DeserializeResponse<List<RoomViewModel>>(response);
        //}

        public async Task<ApiResponse<RoomViewModel>> GetRoomByIdAsync(int id)
        {
                var response = await client.GetAsync($"{apiBaseUrl}rooms/{id}");
                return await DeserializeResponse<RoomViewModel>(response);
        }

        //public async Task<ApiResponse<RoomViewModel>> GetRoomByIdWithAssetsAsync(int id)
        //{
        //        var response = await client.GetAsync($"{apiBaseUrl}rooms/get_room_with_assets/{id}");
        //        return await DeserializeResponse<RoomViewModel>(response);
        //}

        public async Task<ApiResponse<RoomViewModel>> CreateRoomAsync(CreateRoomViewModel room)
        {

                using var form = new MultipartFormDataContent();

                form.Add(new StringContent(room.RoomLocalId?.ToString() ?? ""), "RoomLocalId");
                form.Add(new StringContent(room.RoomName ?? ""), "RoomName");
                form.Add(new StringContent(room.RoomDescription ?? ""), "RoomDescription");
                form.Add(new StringContent(room.IsAndroidBackedUp.ToString()), "IsAndroidBackedUp");
                form.Add(new StringContent(room.IsServerBackedUp.ToString()), "IsServerBackedUp");
                form.Add(new StringContent(room.FloorServerId?.ToString() ?? ""), "FloorServerId");

                if (room.ImageFile != null)
                {
                    var stream = new StreamContent(room.ImageFile.OpenReadStream());
                    stream.Headers.ContentType = new MediaTypeHeaderValue(room.ImageFile.ContentType);
                    form.Add(stream, "ImageFile", room.ImageFile.FileName);
                }

                var response = await client.PostAsync($"{apiBaseUrl}rooms", form);
                return await DeserializeResponse<RoomViewModel>(response);
        }

        public async Task<ApiResponse<RoomViewModel>> UpdateRoomAsync(UpdateRoomViewModel room)
        {
                using var form = new MultipartFormDataContent();

                form.Add(new StringContent(room.RoomServerId.ToString()), "RoomServerId");
                form.Add(new StringContent(room.RoomLocalId?.ToString() ?? ""), "RoomLocalId");
                form.Add(new StringContent(room.RoomName ?? ""), "RoomName");
                form.Add(new StringContent(room.RoomDescription ?? ""), "RoomDescription");
                form.Add(new StringContent(room.IsAndroidBackedUp.ToString()), "IsAndroidBackedUp");
                form.Add(new StringContent(room.IsServerBackedUp.ToString()), "IsServerBackedUp");
                form.Add(new StringContent(room.FloorServerId?.ToString() ?? ""), "FloorServerId");

                if (room.ImageFile != null)
                {
                    var stream = new StreamContent(room.ImageFile.OpenReadStream());
                    stream.Headers.ContentType = new MediaTypeHeaderValue(room.ImageFile.ContentType);
                    form.Add(stream, "ImageFile", room.ImageFile.FileName);
                }

                var response = await client.PutAsync($"{apiBaseUrl}rooms", form);
                return await DeserializeResponse<RoomViewModel>(response);

        }

        public async Task<ApiResponse<string>> DeleteRoomAsync(int id)
        {
            var response = await client.DeleteAsync($"{apiBaseUrl}rooms/{id}");
            return await DeserializeResponse<string>(response);
        }

        /* Helper method */
        private async Task<ApiResponse<T>> DeserializeResponse<T>(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<T>>(body);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Error = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase} - {result?.Error ?? body}"
                    };
                }

                return result ?? new ApiResponse<T> { Success = false, Error = "Empty API response." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Success = false, Error = ex.Message };
            }
        }
    }
}
