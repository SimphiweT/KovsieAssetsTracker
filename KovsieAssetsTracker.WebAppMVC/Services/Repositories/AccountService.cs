using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Account;
using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace KovsieAssetsTracker.WebAppMVC.Services.Repositories
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient client;
        private readonly ApiSettings apiSettings;
        private readonly SessionManager sessionManager;
        private readonly string apiBaseUrl;

        public AccountService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
        {
            client = _client;
            apiSettings = _apiSettings.Value;
            sessionManager = _sessionManager;
            apiBaseUrl = $"{apiSettings.BaseUrl}";
        }

        /* =====================================================
                          AUTHENTICATION
           ===================================================== */
        public async Task<ApiResponse<LoginResponseViewModel>> LoginAsync(LoginViewModel login)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                var content = new StringContent(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}auth/login", content);
                var apiResponseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<LoginResponseViewModel> { Success = false, Error = await BuildErrorMessage(response) };
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<LoginResponseViewModel>>(apiResponseText);
                if (result?.Data?.Token != null)
                {
                    sessionManager.SetSession(result.Data.Token, result.Data.UserRole, result.Data.UserId,
                        result.Data.UserDisplayName, result.Data.UserEmail, result.Data.ProfilePictureUrl);
                }

                return result ?? new ApiResponse<LoginResponseViewModel> { Success = false, Error = "Invalid API response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponseViewModel> { Success = false, Error = ex.Message };
            }
        }

        public Task<string> LogoutAsync()
        {
            sessionManager.Clear();
            return Task.FromResult("Logged out successfully");
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordViewModel resetPassword)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                var content = new StringContent(JsonConvert.SerializeObject(resetPassword), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}auth/reset-password", content);
                var apiText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new ApiResponse<string> { Success = false, Error = await BuildErrorMessage(response) };

                var result = JsonConvert.DeserializeObject<ApiResponse<string>>(apiText);
                return result ?? new ApiResponse<string> { Success = false, Error = "Invalid API response" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordViewModel forgotPassword)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                var content = new StringContent(JsonConvert.SerializeObject(forgotPassword), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}auth/forgot-password", content);
                var apiText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new ApiResponse<string> { Success = false, Error = await BuildErrorMessage(response) };

                var result = JsonConvert.DeserializeObject<ApiResponse<string>>(apiText);
                return result ?? new ApiResponse<string> { Success = false, Error = "Invalid API response" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, Error = ex.Message };
            }
        }

        /* =====================================================
                         ACCESS REQUESTS
           ===================================================== */
        public async Task<ApiResponse<string>> RequestAccessAsync(SignUpViewModel signUp)
        {
            try
            {
                using var content = new MultipartFormDataContent
                {
                    { new StringContent(signUp.DisplayName), "DisplayName" },
                    { new StringContent(signUp.Email), "Email" },
                    { new StringContent(signUp.Role), "RoleApplied" },
                    { new StringContent(signUp.Message ?? ""), "Message" }
                };

                if (signUp.Document != null && signUp.Document.Length > 0)
                {
                    await using var fileStream = signUp.Document.OpenReadStream();
                    var fileContent = new StreamContent(fileStream)
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue("application/pdf") }
                    };
                    content.Add(fileContent, "PdfDocument", signUp.Document.FileName);
                }

                var response = await client.PostAsync($"{apiBaseUrl}auth/request-access", content);
                var apiResponseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new ApiResponse<string> { Success = false, Error = await BuildErrorMessage(response) };

                var result = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponseText);
                return result ?? new ApiResponse<string> { Success = false, Error = "Unexpected response format from API." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, Error = $"Error submitting access request: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<List<AccessResponseViewModel>>> GetAllAccessRequestsAsync()
        {
            try
            {
                var token = sessionManager.GetToken();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{apiBaseUrl}auth/access-requests");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new ApiResponse<List<AccessResponseViewModel>> { Success = false, Error = await BuildErrorMessage(response) };

                var result = JsonConvert.DeserializeObject<ApiResponse<List<AccessResponseViewModel>>>(json);
                return result ?? new ApiResponse<List<AccessResponseViewModel>> { Success = false, Error = "Invalid or empty response from API." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<AccessResponseViewModel>> { Success = false, Error = $"Error fetching access requests: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<string>> ApproveAccessRequestAsync(int id)
        {
            try
            {
                var token = sessionManager.GetToken();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PutAsync($"{apiBaseUrl}auth/approve-access/{id}", null);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new ApiResponse<string> { Success = false, Error = await BuildErrorMessage(response) };

                var result = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                return result ?? new ApiResponse<string> { Success = false, Error = "Unexpected response from API." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponse<string>> DeclineAccessRequestAsync(int id, string reason)
        {
            try
            {
                var token = sessionManager.GetToken();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(JsonConvert.SerializeObject(reason ?? ""), Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{apiBaseUrl}auth/decline/{id}", content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new ApiResponse<string> { Success = false, Error = await BuildErrorMessage(response) };

                var result = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                return result ?? new ApiResponse<string> { Success = false, Error = "Unexpected response from API." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, Error = ex.Message };
            }
        }

        /* =====================================================
                             HELPERS
           ===================================================== */
        private async Task<string> BuildErrorMessage(HttpResponseMessage response)
        {
            string error = await response.Content.ReadAsStringAsync();
            return $"({(int)response.StatusCode}) {response.ReasonPhrase}: {error}";
        }
    }
}
