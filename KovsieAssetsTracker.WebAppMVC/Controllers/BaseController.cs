using KovsieAssetsTracker.WebAppMVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KovsieAssetsTracker.WebAppMVC.Controllers
{
    /// <summary>
    /// Base controller that handles shared session, layout, and access control logic.
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected readonly SessionManager sessionManager;

        protected BaseController(SessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
        }

        /// <summary>
        /// Forces the correct layout for the current role before rendering any view.
        /// </summary>
        protected void ForceLayout()
        {
            var role = sessionManager.GetRole();
            var layoutPath = LayoutHelper.GetLayoutForRole(role);

            // Attach to ViewData as well — Razor always checks this first.
            ViewData["Layout"] = layoutPath;
            ViewBag.Layout = layoutPath;

            Console.WriteLine($"[FORCE-LAYOUT] Role = {role}, Using = {layoutPath}");
        }

        /// <summary>
        /// Explicit layout setter (used for login/reset/etc.).
        /// </summary>
        protected void ForceLayoutForRole(string? role)
        {
            var layoutPath = LayoutHelper.GetLayoutForRole(role);
            ViewData["Layout"] = layoutPath;
            ViewBag.Layout = layoutPath;
        }

        protected bool IsAuthenticated() =>
            !string.IsNullOrEmpty(sessionManager.GetToken());

        protected string GetUserRole() =>
            sessionManager.GetRole() ?? string.Empty;

        /// <summary>
        /// Checks that the current user has the required role, else redirects to Login.
        /// </summary>
        protected IActionResult? RequireRole(string requiredRole)
        {
            var role = GetUserRole();

            if (string.IsNullOrEmpty(role))
            {
                TempData.SetAlert("warning", "You must log in first.", "Access Denied");
                return RedirectToAction("Login", "Home");
            }

            if (!string.Equals(role, requiredRole, StringComparison.OrdinalIgnoreCase))
            {
                TempData.SetAlert("error", "Access denied for your role.", "Unauthorized");
                return RedirectToAction("Login", "Home");
            }

            return null;
        }
    }
}
