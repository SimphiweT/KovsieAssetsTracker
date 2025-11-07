using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Account;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KovsieAssetsTracker.WebAppMVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IAccountService accountService;
        private readonly IUserService userService;
        private readonly IHttpClientFactory httpClientFactory;

        public HomeController(IAccountService _accountService,IUserService _userService,SessionManager _sessionManager, IHttpClientFactory _httpClientFactory) : base(_sessionManager)
        {
            accountService = _accountService;
            userService = _userService;
            httpClientFactory = _httpClientFactory;
        }

        // ======================================================
        // PAGES (Dynamic Layout)
        // ======================================================

        public async Task<IActionResult> Index()
        {
            ForceLayout();
            TempData.SetAlert("info", "Welcome to Kovsie Home!", "Home", "/animation/welcome.json");

            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5250/"); // 🔹 use correct protocol
                var response = await client.GetAsync("api/download/apk-info");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apkInfo = Newtonsoft.Json.Linq.JObject.Parse(json);

                    ViewBag.ApkVersion = apkInfo["version"]?.ToString() ?? "N/A";
                    ViewBag.ApkSize = apkInfo["size"]?.ToString() ?? "N/A";
                    ViewBag.DownloadUrl = apkInfo["downloadUrl"]?.ToString() ?? "#";
                }
                else
                {
                    ViewBag.ApkVersion = "N/A";
                    ViewBag.ApkSize = "N/A";
                    ViewBag.DownloadUrl = "#";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching APK info: {ex.Message}");
                ViewBag.ApkVersion = "N/A";
                ViewBag.ApkSize = "N/A";
                ViewBag.DownloadUrl = "#";
            }

            return View();
        }


        public IActionResult About()
        {
            ForceLayout();
            TempData.SetAlert("question", "About page loaded!", "About Us");
            return View();
        }

        public IActionResult Contact()
        {
            ForceLayout();
            TempData.SetAlert("info", "Contact page loaded!", "Contact Us");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string message)
        {
            ForceLayout();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData.SetAlert("error", "Please fill in all required fields.", "Contact Form");
                return View();
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("http://localhost:5250/");

                var payload = new
                {
                    Name = name,
                    Email = email,
                    Message = message
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/contact/send", content);

                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseBody);

                if (response.IsSuccessStatusCode && result.success == true)
                {
                    TempData.SetAlert("success", (string)result.message ?? "Message sent successfully!", "Contact Us");
                    ModelState.Clear();
                }
                else
                {
                    TempData.SetAlert("error", (string)result.error ?? "Failed to send message.", "Contact Us");
                }
            }
            catch (Exception ex)
            {
                TempData.SetAlert("error", $"An error occurred: {ex.Message}", "Contact Us");
            }

            return View();
        }

        // ======================================================
        // LOGIN / AUTHENTICATION
        // ======================================================

        [HttpGet]
        public IActionResult Login()
        {
            ForceLayoutForRole(null); 
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                ForceLayoutForRole(null);
                return View(login);
            }

            var result = accountService.LoginAsync(login).Result;

            if (result == null || result.Data == null || string.IsNullOrEmpty(result.Data.Token))
            {
                TempData.SetAlert("error", "Login failed!", "Authentication Error");
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                ForceLayoutForRole(null);
                return View(login);
            }

            // Save to session (default)
            sessionManager.SetSession(
                result.Data.Token,
                result.Data.UserRole,
                result.Data.UserId,
                result.Data.UserDisplayName,
                result.Data.UserEmail,
                result.Data.ProfilePictureUrl
            );

            ////Handle Remember Me — store JWT in cookie
            //if (login.RememberMe)
            //{
            //    Response.Cookies.Append("KovsieAuthToken", result.Data.Token, new CookieOptions
            //    {
            //        //Expires = DateTimeOffset.Now.AddDays(7), // or match token expiry
            //        Expires = result.Data.TokenExpirationTime,
            //        HttpOnly = true,
            //        Secure = false, // set to true in production (HTTPS only)
            //        SameSite = SameSiteMode.Strict,
            //        IsEssential = true
            //    });
            //}

            TempData.SetAlert("success", $"Login successful!... Welcome {sessionManager.GetUserDisplayName()} ", "Welcome");

            return result.Data.UserRole switch
            {
                "Administrator" => RedirectToAction("AdminDashboard", "Admin"),
                "VerificationAgent" => RedirectToAction("AgentDashboard", "Agent"),
                "StaffMember" => RedirectToAction("StaffDashboard", "Staff"),
                _ => RedirectToAction("Index", "Home")
            };
        }
        //public override async void OnActionExecuting(ActionExecutingContext context)
        //{
        //    base.OnActionExecuting(context);

        //    var token = sessionManager.GetToken();
        //    var refreshToken = sessionManager.GetRefreshToken();

        //    if (string.IsNullOrEmpty(token) &&
        //        context.HttpContext.Request.Cookies.TryGetValue("KovsieAuthToken", out var cookieToken))
        //    {
        //        token = cookieToken;
        //    }

        //    if (string.IsNullOrEmpty(token)) return;

        //    try
        //    {
        //        var jwtHandler = new JwtSecurityTokenHandler();
        //        var jwt = jwtHandler.ReadJwtToken(token);
        //        var expiry = jwt.ValidTo;

        //        // Refresh 5 minutes before expiry
        //        if (expiry < DateTime.UtcNow.AddMinutes(5) && !string.IsNullOrEmpty(refreshToken))
        //        {
        //            var result = await accountService.RefreshTokenAsync(token, refreshToken);
        //            if (result.Success && result.Data != null)
        //            {
        //                sessionManager.SetSession(
        //                    result.Data.Token,
        //                    result.Data.UserRole,
        //                    result.Data.UserId,
        //                    result.Data.UserDisplayName,
        //                    result.Data.UserEmail,
        //                    result.Data.ProfilePictureUrl,
        //                    result.Data.RefreshToken
        //                );

        //                // Update cookie if RememberMe used
        //                context.HttpContext.Response.Cookies.Append("KovsieAuthToken", result.Data.Token, new CookieOptions
        //                {
        //                    Expires = result.Data.TokenExpirationTime,
        //                    HttpOnly = true,
        //                    Secure = true,
        //                    SameSite = SameSiteMode.Strict
        //                });
        //            }
        //            else
        //            {
        //                sessionManager.Clear();
        //                context.HttpContext.Response.Cookies.Delete("KovsieAuthToken");
        //                context.Result = new RedirectToActionResult("Login", "Home", null);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[RefreshToken] {ex.Message}");
        //    }
        //}




        // ======================================================
        // PASSWORD MANAGEMENT
        // ======================================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ForceLayoutForRole(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Email))
            {
                TempData.SetAlert("error", "Please enter a valid email.", "Forgot Password");
                ForceLayoutForRole(null);
                return View(model);
            }

            var result = await accountService.ForgotPasswordAsync(model);

            if (result.Success)
                TempData.SetAlert("success", result.Message ?? "If an account exists, a reset link has been sent.", "Forgot Password");
            else
                TempData.SetAlert("error", result.Error ?? "Failed to send reset link.", "Forgot Password");

            ForceLayoutForRole(null);
            return View();
        }


        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            ForceLayoutForRole(null);
            var model = new ResetPasswordViewModel { Email = email ?? "", Token = token ?? "" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Please fill all fields correctly.", "Reset Password");
                ForceLayoutForRole(null);
                return View(model);
            }

            var result = await accountService.ResetPasswordAsync(model);

            if (result.Success)
            {
                TempData.SetAlert("success", "Password reset successful! You can now log in.", "Reset Password");
                return RedirectToAction("Login", "Home");
            }

            TempData.SetAlert("error", result.Error ?? "Password reset failed!", "Reset Password");
            ForceLayoutForRole(null);
            return View(model);
        }



        // ======================================================
        // REQUEST ACCESS
        // ======================================================

        [HttpGet]
        public IActionResult RequestAccess()
        {
            ForceLayoutForRole(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestAccess(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ForceLayoutForRole(null);
                return View(model);
            }

            try
            {
                var result = await accountService.RequestAccessAsync(model);

                if (result.Success)
                {
                    TempData.SetAlert("success", "Request submitted successfully!", "Request Access");
                    return RedirectToAction("Login");
                }

                TempData.SetAlert("error", result.Error ?? "Request submission failed!", "Request Access");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                TempData.SetAlert("error", "Unexpected error occurred.", "Request Access");
            }

            ForceLayoutForRole(null);
            return View(model);
        }







        //[HttpGet]
        //public async Task<IActionResult> DownloadApk()
        //{
        //    try
        //    {
        //        using var client = new HttpClient();
        //        client.BaseAddress = new Uri("https://localhost:5250/");
        //        var response = await client.GetAsync("api/download/apk-info");

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            TempData.SetAlert("error", "Unable to fetch APK details.", "Download Error");
        //            return RedirectToAction("Index");
        //        }

        //        var json = await response.Content.ReadAsStringAsync();
        //        dynamic apkInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        //        ViewBag.ApkVersion = apkInfo.version;
        //        ViewBag.ApkSize = apkInfo.size;
        //        ViewBag.DownloadUrl = apkInfo.downloadUrl;

        //        return View("DownloadApk");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData.SetAlert("error", $"Download failed: {ex.Message}", "Download Error");
        //        return RedirectToAction("Index");
        //    }
        //}

    }
}
