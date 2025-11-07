using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Models.User;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Configuration;
using System.Net.Http.Headers;
using System.Text;

namespace KovsieAssetsTracker.WebAppMVC.Services.Repositories
{
    public class UserService : IUserService
    {
        private HttpClient client;
        private readonly ApiSettings apiSettings;
        private readonly SessionManager sessionManager;
        private readonly string apiBaseUrl;

        public UserService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
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
        public async Task<ApiResponse<List<UserViewModel>>> GetAllUsersAsync()
        {

            var response = await client.GetAsync($"{apiBaseUrl}users");

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<List<UserViewModel>>
                {
                    Success = false,
                    Error = await BuildErrorMessage(response)
                };
            }

            var apiResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<List<UserViewModel>>>(apiResponse);

            return new ApiResponse<List<UserViewModel>>
            {
                Success = result.Success,
                Data = result.Data,
                Message = result.Message
            };
        }

        public async Task<ApiResponse<UserViewModel>> GetUserByIdAsync(string userId)
        {

            var response = await client.GetAsync($"{apiBaseUrl}users/get_user/{userId}");
            //var response = await client.GetAsync($"{apiBaseUrl}users/{userId}");
            Console.WriteLine($"GET => {apiBaseUrl}users/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<UserViewModel>
                {
                    Success = false,
                    Error = await BuildErrorMessage(response)
                };
            }

            var apiResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<UserViewModel>>(apiResponse);

            return new ApiResponse<UserViewModel>
            {
                Success = result.Success,
                Message = result.Message,
                Error = result.Error,
                Data = result.Data,
            };

        }

        public async Task<ApiResponse<UserViewModel>> RegisterUserAsync(CreateUserViewModel newUser)
        {
            ApiResponse<UserViewModel>? result;

            using (var formContent = new MultipartFormDataContent())
            {
                // Text fields
                formContent.Add(new StringContent(newUser.UsernameDisplay), "UsernameDisplay");
                formContent.Add(new StringContent(newUser.UserEmail), "UserEmail");
                formContent.Add(new StringContent(newUser.StudentStaffNumber ?? string.Empty), "StudentStaffNumber");
                formContent.Add(new StringContent(newUser.ContactNumber ?? string.Empty), "ContactNumber");
                formContent.Add(new StringContent(newUser.Description ?? string.Empty), "Description");
                formContent.Add(new StringContent(newUser.UserRole), "UserRole");
                formContent.Add(new StringContent(newUser.Password), "Password");

                // File field (if user uploaded profile image)
                if (newUser.ImageFile != null && newUser.ImageFile.Length > 0)
                {
                    using var stream = newUser.ImageFile.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(newUser.ImageFile.ContentType);
                    formContent.Add(fileContent, "ImageFile", newUser.ImageFile.FileName);
                }

                // Send request
                var response = await client.PostAsync($"{apiBaseUrl}auth/register", formContent);
                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<UserViewModel>
                    {
                        Success = false,
                        Error = await BuildErrorMessage(response)
                    };
                }

                var apiResponse = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<ApiResponse<UserViewModel>>(apiResponse);
            }

            return new ApiResponse<UserViewModel>
            {
                Success = result.Success,
                Data = result.Data,
                Message = result.Message
            };

        }

        public async Task<ApiResponse<UserViewModel>> UpdateUserAsync(UpdateUserViewModel updatedUser)
        {
            ApiResponse<UserViewModel>? result;

            using var formContent = new MultipartFormDataContent();

            formContent.Add(new StringContent(updatedUser.UserId ?? string.Empty), "UserId");
            formContent.Add(new StringContent(updatedUser.UsernameDisplay ?? string.Empty), "UsernameDisplay");
            formContent.Add(new StringContent(updatedUser.UserEmail ?? string.Empty), "UserEmail");
            formContent.Add(new StringContent(updatedUser.StudentStaffNumber ?? string.Empty), "StudentStaffNumber");
            formContent.Add(new StringContent(updatedUser.ContactNumber ?? string.Empty), "ContactNumber");
            formContent.Add(new StringContent(updatedUser.Description ?? string.Empty), "Description");
            formContent.Add(new StringContent(updatedUser.UserRole ?? string.Empty), "UserRole");

            if (updatedUser.ImageFile != null && updatedUser.ImageFile.Length > 0)
            {
                using var stream = updatedUser.ImageFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(updatedUser.ImageFile.ContentType);
                formContent.Add(fileContent, "ImageFile", updatedUser.ImageFile.FileName);
            }

            if (string.IsNullOrWhiteSpace(updatedUser.UserId))
            {
                return new ApiResponse<UserViewModel>
                {
                    Success = false,
                    Error = "UserId is missing when updating user."
                };
            }
            Console.WriteLine($"PUT => {apiBaseUrl}users/{updatedUser.UserId}");
            Console.WriteLine($"Fields => {JsonConvert.SerializeObject(updatedUser)}");
            //var response = await client.PutAsync($"{apiBaseUrl}users/update_by_id/{updatedUser.UserId}", formContent);
            var response = await client.PutAsync($"{apiBaseUrl}users/{updatedUser.UserId}", formContent);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<UserViewModel>
                {
                    Success = false,
                    Error = await BuildErrorMessage(response)
                };
            }

            var apiResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {response.StatusCode} - {apiResponse}");
            result = JsonConvert.DeserializeObject<ApiResponse<UserViewModel>>(apiResponse);

            return new ApiResponse<UserViewModel>
            {
                Success = result.Success,
                Data = result.Data,
                Message = result.Message
            };
        }


        public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordViewModel changePassword)
        {
            var content = new StringContent(JsonConvert.SerializeObject(changePassword), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{apiBaseUrl}users/change-password", content);
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

            return new ApiResponse<string>
            {
                Success = result.Success,
                Data = result.Data,
                Message = result.Message
            };

        }
        public async Task<ApiResponse<string>> AdminResetPasswordAsync(string email, string temporaryPassword)
        {
            using var form = new MultipartFormDataContent
        {
        { new StringContent(email), "email" },
        { new StringContent(temporaryPassword), "temporaryPassword" }
        };

            var response = await client.PostAsync($"{apiBaseUrl}users/admin-reset-password", form);
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Error = await BuildErrorMessage(response)
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
            return result ?? new ApiResponse<string> { Success = false, Error = "Invalid API response." };
        }


        public async Task<ApiResponse<string>> DeleteUserAsync(string userId)
        {
            var response = await client.DeleteAsync($"{apiBaseUrl}users/{userId}");

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

            return new ApiResponse<string>
            {
                Success = result.Success,
                Data = result.Data,
                Message = result.Message
            };
        }
        private async Task<string> BuildErrorMessage(HttpResponseMessage response)
        {
            string error = await response.Content.ReadAsStringAsync();
            return $"API request failed with status code {response.StatusCode}: {error}";
        }
    }
}
