using KovsieAssetsTracker.WebAppMVC.Models.Account;
using KovsieAssetsTracker.WebAppMVC.Models.User;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KovsieAssetsTracker.WebAppMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService accountService;
        private readonly IUserService userService;

        public AccountController(IAccountService _accountService, IUserService _userService)
        {
            accountService = _accountService;
            userService = _userService;
        }

        // ---------------- LOGIN ----------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (ModelState.IsValid)
            {
                var result = await accountService.LoginAsync(login);

                if (result.Success && result.Data != null)
                {
                    var role = result.Data.UserRole;

                    TempData["SweetAlertType"] = "success";
                    TempData["SweetAlertMessage"] = "Login successful!";
                    TempData["LottieAnimation"] = role; // "Administrator" | "Technician"

                    if (role == "Administrator")
                        return RedirectToAction("AdminDashboard", "Admin");
                    else if (role == "StaffMember")
                        return RedirectToAction("AgentDashboard", "Technician");
                    else if(role == "VerificationAgent")
                        return RedirectToAction("AgentDashboard", "Technician");

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["SweetAlertType"] = "error";
                    TempData["SweetAlertMessage"] = result.Error ?? "Login failed.";
                    TempData["LottieAnimation"] = "Error";
                }
            }

            return View(login);
        }


        // ---------------- RESET PASSWORD ----------------
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPassword)
        {
            if (ModelState.IsValid)
            {
                var result = await accountService.ResetPasswordAsync(resetPassword);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Password reset successfully!";
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError(string.Empty, result.Error ?? "Password reset failed.");
            }

            return View(resetPassword);
        }

        // ---------------- CHANGE PASSWORD ----------------
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await userService.ChangePasswordAsync(model);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, result.Error ?? "Password change failed.");
            }

            return View(model);
        }

        // ---------------- DELETE ACCOUNT ----------------
        [HttpPost]
        public async Task<IActionResult> DeleteAccount(string userId)
        {
            var result = await userService.DeleteUserAsync(userId);
            if (result.Success)
            {
                await accountService.LogoutAsync();
                TempData["SuccessMessage"] = "Account deleted successfully.";
                return RedirectToAction("Login");
            }

            TempData["ErrorMessage"] = result.Error ?? "Failed to delete account.";
            return RedirectToAction("Index", "Home");
        }

        // ---------------- LOGOUT ----------------
        public async Task<IActionResult> Logout()
        {
            await accountService.LogoutAsync();
            return RedirectToAction("Login");
        }
    }
}
