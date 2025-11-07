using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace KovsieAssetsTracker.WebAppMVC.Helpers
{
    public class SessionManager
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        // Session Keys
        private const string TokenKey = "JwtToken";
        private const string RoleKey = "UserRole";
        private const string UserIdKey = "UserId";
        private const string UserDisplayNameKey = "UserDisplayName";
        private const string UserEmailKey = "UserEmail";
        private const string ProfilePictureUrlKey = "ProfilePictureUrl";

        public SessionManager(IHttpContextAccessor _httpContextAccessor)
        {
            httpContextAccessor = _httpContextAccessor;
        }

        // ===============================================================
        //  SETTERS
        // ===============================================================

        /// <summary>
        /// Sets all major session values after successful login.
        /// </summary>
        public void SetSession(string token, string role, string userId, string displayName, string email, string profileUrl)
        {
            var ctx = httpContextAccessor.HttpContext;
            ctx?.Session.SetString(TokenKey, token ?? "");
            ctx?.Session.SetString(RoleKey, role ?? "");
            ctx?.Session.SetString(UserIdKey, userId ?? "");
            ctx?.Session.SetString(UserDisplayNameKey, displayName ?? "");
            ctx?.Session.SetString(UserEmailKey, email ?? "");
            ctx?.Session.SetString(ProfilePictureUrlKey, profileUrl ?? "~/images/default-avatar.png");
        }

        /// <summary>
        /// Updates only the display name in session (used after profile edits).
        /// </summary>
        public void SetUserDisplayName(string name)
        {
            var context = httpContextAccessor.HttpContext;
            if (context == null) return;
            context.Session.SetString(UserDisplayNameKey, name ?? "");
        }

        /// <summary>
        /// Updates the profile image path in session.
        /// </summary>
        public void SetProfilePictureUrl(string imageUrl)
        {
            var context = httpContextAccessor.HttpContext;
            if (context == null) return;
            context.Session.SetString(ProfilePictureUrlKey, imageUrl ?? "");
        }

        /// <summary>
        /// Updates email if user changes it (optional, usually readonly).
        /// </summary>
        public void SetUserEmail(string email)
        {
            var context = httpContextAccessor.HttpContext;
            if (context == null) return;
            context.Session.SetString(UserEmailKey, email ?? "");
        }

        // ===============================================================
        // GETTERS
        // ===============================================================

        public string? GetToken() =>
            httpContextAccessor.HttpContext?.Session.GetString(TokenKey);

        public string? GetRole() =>
            httpContextAccessor.HttpContext?.Session.GetString(RoleKey);

        public string? GetUserId() =>
            httpContextAccessor.HttpContext?.Session.GetString(UserIdKey);

        public string? GetUserDisplayName() =>
            httpContextAccessor.HttpContext?.Session.GetString(UserDisplayNameKey);

        public string? GetUserEmail() =>
            httpContextAccessor.HttpContext?.Session.GetString(UserEmailKey);

        public string? GetProfilePictureUrl()
        {
            var url = httpContextAccessor.HttpContext?.Session.GetString(ProfilePictureUrlKey);
            return string.IsNullOrWhiteSpace(url)
                ? "/images/default-avatar.png"
                : url;
        }

        // ===============================================================
        //  CLEAR SESSION
        // ===============================================================

        public void Clear()
        {
            var context = httpContextAccessor.HttpContext;
            if (context == null) return;

            context.Session.Remove(TokenKey);
            context.Session.Remove(RoleKey);
            context.Session.Remove(UserIdKey);
            context.Session.Remove(UserDisplayNameKey);
            context.Session.Remove(UserEmailKey);
            context.Session.Remove(ProfilePictureUrlKey);
        }
    }
}
