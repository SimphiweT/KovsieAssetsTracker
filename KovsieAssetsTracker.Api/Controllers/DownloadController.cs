using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KovsieAssetsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {

        /// <summary>Downloads the latest Kovsie Assets Tracker APK file.</summary>
        [HttpGet("download-apk")]
        [AllowAnonymous]
        public IActionResult DownloadApk()
        {
            var apkPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/apk/KovsieAssetsTracker.apk");
            if (!System.IO.File.Exists(apkPath))
                return NotFound("APK not found.");

            var bytes = System.IO.File.ReadAllBytes(apkPath);
            return File(bytes, "application/vnd.android.package-archive", "KovsieAssetsTracker.apk");
        }
        /// <summary>Returns the latest version info and file size of the Kovsie APK.</summary>
        [HttpGet("apk-info")]
        [AllowAnonymous]
        public IActionResult GetApkInfo()
        {
            var apkPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/apk/KovsieAssetsTracker.apk");
            if (!System.IO.File.Exists(apkPath))
                return NotFound(new { success = false, message = "APK not found." });

            var fileInfo = new FileInfo(apkPath);
            var fileSizeMB = (fileInfo.Length / (1024.0 * 1024.0)).ToString("F2");

            var apkInfo = new
            {
                success = true,
                version = "v1.0.0",        
                size = $"{fileSizeMB} MB",
                downloadUrl = $"{Request.Scheme}://{Request.Host}/api/download/download-apk"
            };

            return Ok(apkInfo);
        }

    }
}
