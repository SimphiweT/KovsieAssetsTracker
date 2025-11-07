using KovsieAssetsTracker.Application.DTOs.AssetDTOs;
using KovsieAssetsTracker.Application.DTOs.SharedDTOs;
using KovsieAssetsTracker.Application.Helpers;
using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing.Imaging;
using System.Globalization;

namespace KovsieAssetsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssetsController : ControllerBase
    {
        private readonly IRepositoryWrapper repo;
        private readonly IWebHostEnvironment env;

        private static readonly string AssetsFolder = "assets";
        private static readonly string QrCodesFolder = "qrcodes";
        private static readonly string DefaultAssetImage = "default-assets.png";
        private static readonly string DefaultQrImage = "default-qrcode.png";

        public AssetsController(IRepositoryWrapper _repo, IWebHostEnvironment _env)
        {
            repo = _repo;
            env = _env;
        }

        private string GetBaseUrl(string folder) =>
            $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/{folder}/";

        private async Task<string> SaveImageAsync(IFormFile? file, string folderName, string defaultImage)
        {
            if (file == null || file.Length == 0)
                return defaultImage;

            var folder = Path.Combine(env.WebRootPath, "images", folderName);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

        private Task<string> GenerateQrCodeAsync(string qrText)
        {
            return Task.Run(() =>
            {
                try
                {
                    var folder = Path.Combine(env.WebRootPath, "images", QrCodesFolder);
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var fileName = $"{Guid.NewGuid()}_autoqrcode.png";
                    var filePath = Path.Combine(folder, fileName);

                    var qrUrl = $"{Request.Scheme}://{Request.Host}/api/assets/qrcode/{qrText}";

                    using var qrGenerator = new QRCodeGenerator();
                    var qrData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
                    using var qrCode = new QRCode(qrData);
                    using var bitmap = qrCode.GetGraphic(20);

                    bitmap.Save(filePath, ImageFormat.Png);
                    return fileName;
                }
                catch
                {
                    return DefaultQrImage;
                }
            });
        }

        // ============================================================
        // GET: All Assets (with Room + Category)
        // ============================================================
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetAllAssetsWithRoomAndCategory()
        {
            var assets = await repo.Asset.GetAllAssetsWithCategoryAndRoomAsync();
            var response = assets.Select(MapToDto).ToList();

            return Ok(ApiResponseDTO<List<AssetResponseDTO>>.SuccessResponse(
                response, "Assets retrieved successfully..."));
        }

        // ============================================================
        // GET: Asset by ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssetById([FromRoute] int id)
        {
            var asset = await repo.Asset.GetByIdAsync(id);
            if (asset == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Asset not found."));

            return Ok(ApiResponseDTO<AssetResponseDTO>.SuccessResponse(
                MapToDto(asset), "Asset retrieved successfully..."));
        }

        // ============================================================
        // GET: Asset by QR Code
        // ============================================================
        [HttpGet("qrcode/{qrCodeSecretText}")]
        public async Task<IActionResult> GetAssetByQRCode([FromRoute] string qrCodeSecretText)
        {
            var asset = await repo.Asset.GetAssetByQRCodeAsync(qrCodeSecretText);
            if (asset == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Asset not found."));

            return Ok(ApiResponseDTO<AssetResponseDTO>.SuccessResponse(
                MapToDto(asset), "Asset retrieved successfully..."));
        }

        // ============================================================
        // POST: Create Asset
        // ============================================================
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE}")]
        public async Task<IActionResult> CreateAsset([FromForm] CreateAssetRequestDTO dto)
        {
            try
            {
                var assetImage = await SaveImageAsync(dto.ImageFileAsset, AssetsFolder, DefaultAssetImage);
                var qrImage = await SaveImageAsync(dto.ImageFileQrCode, QrCodesFolder, DefaultQrImage);

                string qrSecret = string.IsNullOrWhiteSpace(dto.QRCodeSecretText)
                    ? $"{dto.AssetName}_{Guid.NewGuid():N}".ToLower()
                    : dto.QRCodeSecretText;

                if (qrImage == DefaultQrImage)
                    qrImage = await GenerateQrCodeAsync(qrSecret);

                var asset = new Asset
                {
                    AssetLocalId = dto.AssetLocalId,
                    AssetName = dto.AssetName,
                    AssetDescription = dto.AssetDescription,
                    AssetImage = assetImage,
                    AssetQRCodeImage = qrImage,
                    QRCodeSecretText = qrSecret,
                    Status = dto.Status,
                    IsAndroidBackedUp = dto.IsAndroidBackedUp,
                    IsServerBackedUp = dto.IsServerBackedUp,
                    RoomServerId = dto.RoomServerId,
                    AssetCategoryServerId = dto.AssetCategoryServerId,
                    CreatedDate = DateTime.UtcNow
                };

                await repo.Asset.CreateAsync(asset);
                await repo.SaveAsync();

                return Ok(ApiResponseDTO<AssetResponseDTO>.SuccessResponse(
                    MapToDto(asset), "Asset created successfully..."));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating asset: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, ApiResponseDTO<string>.FailResponse($"Server error: {ex.Message}"));
            }
        }


        // ============================================================
        // PUT: Update Asset
        // ============================================================
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> UpdateAsset([FromRoute] int id, [FromForm] UpdateAssetRequestDTO dto)
        {
            dto.AssetServerId = id;
            var asset = await repo.Asset.GetByIdAsync(id);
            if (asset == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Asset not found."));

            // Images
            if (dto.ImageFileAsset != null)
                asset.AssetImage = await SaveImageAsync(dto.ImageFileAsset, AssetsFolder, DefaultAssetImage);

            if (dto.ImageFileQrCode != null)
                asset.AssetQRCodeImage = await SaveImageAsync(dto.ImageFileQrCode, QrCodesFolder, DefaultQrImage);

            // Fields
            asset.AssetLocalId = dto.AssetLocalId ?? asset.AssetLocalId;
            asset.AssetName = string.IsNullOrWhiteSpace(dto.AssetName) ? asset.AssetName : dto.AssetName;
            asset.AssetDescription = string.IsNullOrWhiteSpace(dto.AssetDescription) ? asset.AssetDescription : dto.AssetDescription;
            asset.QRCodeSecretText = dto.QRCodeSecretText ?? asset.QRCodeSecretText;
            asset.Status = dto.Status ?? asset.Status;
            asset.IsAndroidBackedUp = dto.IsAndroidBackedUp;
            asset.IsServerBackedUp = dto.IsServerBackedUp;
            asset.RoomServerId = dto.RoomServerId ?? asset.RoomServerId;
            asset.AssetCategoryServerId = dto.AssetCategoryServerId ?? asset.AssetCategoryServerId;
            asset.UpdatedDate = DateTime.UtcNow;

            // QR generation
            if (string.IsNullOrWhiteSpace(asset.QRCodeSecretText))
                asset.QRCodeSecretText = $"{asset.AssetName}_{Guid.NewGuid():N}".ToLower();

            if (string.IsNullOrEmpty(asset.AssetQRCodeImage) || asset.AssetQRCodeImage == DefaultQrImage)
                asset.AssetQRCodeImage = await GenerateQrCodeAsync(asset.QRCodeSecretText);

            await repo.Asset.UpdateAsync(asset);
            await repo.SaveAsync();

            return Ok(ApiResponseDTO<AssetResponseDTO>.SuccessResponse(
                MapToDto(asset), "Asset updated successfully..."));
        }

        // ============================================================
        // DELETE: Asset
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> DeleteAsset([FromRoute] int id)
        {
            var asset = await repo.Asset.GetByIdAsync(id);
            if (asset == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Asset not found."));

            DeleteImageIfExists(asset.AssetImage, AssetsFolder, DefaultAssetImage);
            DeleteImageIfExists(asset.AssetQRCodeImage, QrCodesFolder, DefaultQrImage);

            await repo.Asset.DeleteAsync(asset);
            await repo.SaveAsync();

            return Ok(ApiResponseDTO<string>.SuccessResponse("Asset deleted successfully"));
        }

        // ============================================================
        // Helpers
        // ============================================================
        private void DeleteImageIfExists(string? fileName, string folder, string defaultName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == defaultName)
                return;

            var filePath = Path.Combine(env.WebRootPath, "images", folder, fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        private AssetResponseDTO MapToDto(Asset a) => new()
        {
            AssetServerId = a.AssetServerId,
            AssetLocalId = a.AssetLocalId,
            AssetName = a.AssetName,
            AssetDescription = a.AssetDescription,
            AssetImage = GetBaseUrl(AssetsFolder) + a.AssetImage,
            AssetQRCodeImage = GetBaseUrl(QrCodesFolder) + a.AssetQRCodeImage,
            QRCodeSecretText = a.QRCodeSecretText,
            Status = a.Status,
            IsAndroidBackedUp = a.IsAndroidBackedUp,
            IsServerBackedUp = a.IsServerBackedUp,
            CreatedDate = a.CreatedDate,
            UpdatedDate = a.UpdatedDate,
            RoomServerId = a.RoomServerId,
            RoomName = a.Room?.RoomName,
            AssetCategoryServerId = a.AssetCategoryServerId,
            AssetCategoryName = a.AssetCategory?.AssetCategoryName
        };
    }
}
