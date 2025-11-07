using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace KovsieAssetsTracker.WebAppMVC.Helpers
{
    public static class TempDataExtensions
    {
        /// <summary>
        /// Sets a SweetAlert notification and optional Lottie animation.
        /// </summary>
        public static void SetAlert(this ITempDataDictionary tempData, string type, string message, string title = "Notification", string lottieAnimation = null)
        {
            tempData["SweetAlertType"] = type;
            tempData["SweetAlertMessage"] = message;
            tempData["SweetAlertTitle"] = title;

            if (!string.IsNullOrWhiteSpace(lottieAnimation))
            {
                tempData["LottieAnimation"] = lottieAnimation;
            }
        }
    }
}
