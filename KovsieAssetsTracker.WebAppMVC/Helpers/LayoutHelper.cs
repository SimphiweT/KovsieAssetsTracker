namespace KovsieAssetsTracker.WebAppMVC.Helpers
{
    public static class LayoutHelper
    {
        /// <summary>
        /// Returns the correct layout file path for a given role.
        /// </summary>
        public static string GetLayoutForRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "_Layout"; // default

            return role switch
            {
                "Administrator" => "~/Views/Shared/_LayoutAdmin.cshtml",
                "StaffMember" => "~/Views/Shared/_LayoutStaff.cshtml",
                "VerificationAgent" => "~/Views/Shared/_LayoutAgent.cshtml",
                _ => "_Layout"
            };
        }
    }
}
