using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Asset;
using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Models.User;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KovsieAssetsTracker.WebAppMVC.Services.Repositories
{
    public class AssetService : IAssetService
    {
        private HttpClient client;
        private readonly ApiSettings apiSettings;
        private readonly SessionManager sessionManager;
        private readonly string apiBaseUrl;

        private const string GET_ALL = "assets";
        private const string GET_BY_ID = "assets/{0}";
        private const string GET_BY_QR = "assets/qrcode/{0}";
        private const string CREATE = "assets";
        private const string UPDATE = "assets/{0}";
        private const string DELETE = "assets/{0}";


        public AssetService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
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

        /*====================================================================================================================================
                                                         *** GET ALL ASSETS ***
        ======================================================================================================================================*/
        public async Task<ApiResponse<List<AssetViewModel>>> GetAllAssetsAsync()
        {
            try
            {
                //var response = await client.GetAsync($"{apiBaseUrl}assets");
                var response = await client.GetAsync($"{apiBaseUrl}{string.Format(GET_ALL)}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<List<AssetViewModel>>
                    {
                        Success = false,
                        Error = await BuildErrorMessage(response)
                    };
                }
                var apiResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<List<AssetViewModel>>>(apiResponse);

                return new ApiResponse<List<AssetViewModel>>
                {
                    Success = result.Success,
                    Data = result.Data,
                    Message = result.Message
                };
            }
            catch(Exception ex)
            {
                var errorResponse = new ApiResponse<List<AssetViewModel>>
                {
                    Success = false,
                    Error = $"An error occurred while fetching assets: {ex.Message}"
                };
                return errorResponse;
            }
           
        }
        /*====================================================================================================================================
                                                         *** GET ASSET BY ID ***
        ======================================================================================================================================*/
        public async Task<ApiResponse<AssetViewModel>> GetAssetByIdAsync(int id)
        {
            try
            {
                //var response = await client.GetAsync($"{apiBaseUrl}assets/{id}");
                var response = await client.GetAsync($"{apiBaseUrl}{string.Format(GET_BY_ID,id)}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<AssetViewModel>
                    {
                        Success = false,
                        Error = await BuildErrorMessage(response)
                    };
                }
                var apiResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<AssetViewModel>>(apiResponse);

                return new ApiResponse<AssetViewModel>
                {
                    Success = result.Success,
                    Data = result.Data,
                    Message = result.Message
                };
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<AssetViewModel>
                {
                    Success = false,
                    Error = $"An error occurred while fetching assets: {ex.Message}"
                };
                return errorResponse;
            }
        }
        /*====================================================================================================================================
                                                        *** GET ASSET BY QRCODE ***
        ======================================================================================================================================*/
        public async Task<ApiResponse<AssetViewModel>> GetAssetByQrCodeAsync(string qrCode)
        {
            try
            {
                //var response = await client.GetAsync($"{apiBaseUrl}assets/qrcode/{qrCode}");
                var response = await client.GetAsync($"{apiBaseUrl}{string.Format(GET_BY_QR,qrCode)}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<AssetViewModel>
                    {
                        Success = false,
                        Error = await BuildErrorMessage(response)
                    };
                }
                var apiResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<AssetViewModel>>(apiResponse);

                return new ApiResponse<AssetViewModel>
                {
                    Success = result.Success,
                    Data = result.Data,
                    Message = result.Message
                };
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<AssetViewModel>
                {
                    Success = false,
                    Error = $"An error occurred while fetching assets: {ex.Message}"
                };
                return errorResponse;
            }
        }

        /*====================================================================================================================================
                                                           *** CREATE ASSET ***
        ======================================================================================================================================*/
        public async Task<ApiResponse<AssetViewModel>> CreateAssetAsync(CreateAssetViewModel newAsset)
        {
            try
            {
                ApiResponse<AssetViewModel>? result;

                using (var formContent = new MultipartFormDataContent())
                {
                    // Text fields
                    formContent.Add(new StringContent(newAsset.AssetLocalId?.ToString() ?? ""), "AssetLocalId");
                    formContent.Add(new StringContent(newAsset.AssetName ?? ""), "AssetName");
                    formContent.Add(new StringContent(newAsset.AssetDescription ?? ""), "AssetDescription");
                    formContent.Add(new StringContent(newAsset.QRCodeSecretText ?? ""), "QRCodeSecretText");
                    formContent.Add(new StringContent(newAsset.Status ?? ""), "Status");
                    formContent.Add(new StringContent(newAsset.IsAndroidBackedUp.ToString() ?? ""), "IsAndroidBackedUp");
                    formContent.Add(new StringContent(newAsset.IsServerBackedUp.ToString() ?? ""), "IsServerBackedUp");
                    formContent.Add(new StringContent(newAsset.RoomServerId?.ToString() ?? ""), "RoomServerId");
                    formContent.Add(new StringContent(newAsset.AssetCategoryServerId?.ToString() ?? ""), "AssetCategoryServerId");

                    // File field (Asset Image)
                    if (newAsset.ImageFileAsset != null && newAsset.ImageFileAsset.Length > 0)
                    {
                        var fileContent = new StreamContent(newAsset.ImageFileAsset.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(newAsset.ImageFileAsset.ContentType);
                        formContent.Add(fileContent, "ImageFileAsset", newAsset.ImageFileAsset.FileName);
                    }

                    // File field (QR Image)
                    if (newAsset.ImageFileQrCode != null && newAsset.ImageFileQrCode.Length > 0)
                    {
                        var fileContent = new StreamContent(newAsset.ImageFileQrCode.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(newAsset.ImageFileQrCode.ContentType);
                        formContent.Add(fileContent, "ImageFileQrCode", newAsset.ImageFileQrCode.FileName);
                    }

                    //var response = await client.PostAsync($"{apiBaseUrl}assets", formContent);
                    var response = await client.PostAsync($"{apiBaseUrl}{string.Format(CREATE)}", formContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        return new ApiResponse<AssetViewModel>
                        {
                            Success = false,
                            Error = await BuildErrorMessage(response)
                        };
                    }

                    var apiResponse = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<ApiResponse<AssetViewModel>>(apiResponse);
                }

                return result ?? new ApiResponse<AssetViewModel> { Success = false, Error = "Empty response" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AssetViewModel>
                {
                    Success = false,
                    Error = $"An error occurred while creating asset: {ex.Message}"
                };
            }

        }

        /*====================================================================================================================================
                                                          *** UPDATE ASSET ***
        ======================================================================================================================================*/

        public async Task<ApiResponse<AssetViewModel>> UpdateAssetAsync(UpdateAssetViewModel asset)
        {
            try
            {
                ApiResponse<AssetViewModel>? result;

                using (var formContent = new MultipartFormDataContent())
                {
                    formContent.Add(new StringContent(asset.AssetServerId.ToString()), "AssetServerId");
                    formContent.Add(new StringContent(asset.AssetLocalId?.ToString() ?? ""), "AssetLocalId");
                    formContent.Add(new StringContent(asset.AssetName ?? ""), "AssetName");
                    formContent.Add(new StringContent(asset.AssetDescription ?? ""), "AssetDescription");
                    formContent.Add(new StringContent(asset.QRCodeSecretText ?? ""), "QRCodeSecretText");
                    formContent.Add(new StringContent(asset.Status ?? ""), "Status");
                    formContent.Add(new StringContent(asset.IsAndroidBackedUp.ToString()), "IsAndroidBackedUp");
                    formContent.Add(new StringContent(asset.IsServerBackedUp.ToString()), "IsServerBackedUp");
                    formContent.Add(new StringContent(asset.RoomServerId?.ToString() ?? ""), "RoomServerId");
                    formContent.Add(new StringContent(asset.AssetCategoryServerId?.ToString() ?? ""), "AssetCategoryServerId");

                    if (asset.ImageFileAsset != null && asset.ImageFileAsset.Length > 0)
                    {
                        var fileContent = new StreamContent(asset.ImageFileAsset.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(asset.ImageFileAsset.ContentType);
                        formContent.Add(fileContent, "ImageFileAsset", asset.ImageFileAsset.FileName);
                    }

                    if (asset.ImageFileQrCode != null && asset.ImageFileQrCode.Length > 0)
                    {
                        var fileContent = new StreamContent(asset.ImageFileQrCode.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(asset.ImageFileQrCode.ContentType);
                        formContent.Add(fileContent, "ImageFileQrCode", asset.ImageFileQrCode.FileName);
                    }

                    //var response = await client.PutAsync($"{apiBaseUrl}assets/{asset.AssetServerId}", formContent);
                    var response = await client.PutAsync($"{apiBaseUrl}{string.Format(UPDATE,asset.AssetServerId)}", formContent);
                    Console.WriteLine(response.Content);
                    if (!response.IsSuccessStatusCode)
                    {
                        return new ApiResponse<AssetViewModel>
                        {
                            Success = false,
                            Error = await BuildErrorMessage(response)
                        };
                    }

                    var apiResponse = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<ApiResponse<AssetViewModel>>(apiResponse);
                }

                return result ?? new ApiResponse<AssetViewModel> { Success = false, Error = "Empty response" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AssetViewModel>
                {
                    Success = false,
                    Error = $"An error occurred while updating asset: {ex.Message}"
                };
            }
        }

        /*====================================================================================================================================
                                                          *** DELETE ASSET ***
        ======================================================================================================================================*/
        public async Task<ApiResponse<string>> DeleteAssetAsync(int id)
        {
            try
            {
                var response = await client.DeleteAsync($"{apiBaseUrl}assets/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Error = await BuildErrorMessage(response)
                    };
                }

                var apiResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse);

                return result ?? new ApiResponse<string> { Success = false, Error = "Empty response" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Error = $"An error occurred while deleting asset: {ex.Message}"
                };
            }
        }




        /*====================================================================================================================================
                                                        *** HELPERS ***
        ======================================================================================================================================*/
        private async Task<string> BuildErrorMessage(HttpResponseMessage response)
        {
            string error = await response.Content.ReadAsStringAsync();
            return $"API request failed with status code {response.StatusCode}: {error}";
        }
    }
}
