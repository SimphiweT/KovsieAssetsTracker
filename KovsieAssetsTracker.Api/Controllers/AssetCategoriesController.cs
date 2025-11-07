using KovsieAssetsTracker.Application.DTOs.AssetCategoryDTOs;
using KovsieAssetsTracker.Application.DTOs.AssetDTOs;
using KovsieAssetsTracker.Application.DTOs.SharedDTOs;
using KovsieAssetsTracker.Application.Helpers;
using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KovsieAssetsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssetCategoriesController : ControllerBase
    {
        private readonly IRepositoryWrapper repo;
        private readonly IWebHostEnvironment env;

        private static readonly string FolderName = "asset_categories";
        private static readonly string DefaultImage = "default-assetcategory.png";

        public AssetCategoriesController(IRepositoryWrapper _repo, IWebHostEnvironment _env)
        {
            repo = _repo;
            env = _env;
        }

        // ============================================================
        // Helpers
        // ============================================================
        private string GetBaseUrl(string folder) =>
            $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/{folder}/";

        private async Task<string> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return DefaultImage;

            var folder = Path.Combine(env.WebRootPath, "images", FolderName);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

        private void DeleteImageIfExists(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == DefaultImage)
                return;

            var filePath = Path.Combine(env.WebRootPath, "images", FolderName, fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        private AssetCategoryResponseDTO MapToDto(AssetCategory ac) => new()
        {
            AssetCategoryServerId = ac.AssetCategoryServerId,
            AssetCategoryLocalId = ac.AssetCategoryLocalId,
            AssetCategoryName = ac.AssetCategoryName,
            AssetCategoryDescription = ac.AssetCategoryDescription,
            AssetCategoryImage = GetBaseUrl(FolderName) + (string.IsNullOrEmpty(ac.AssetCategoryImage) ? DefaultImage : ac.AssetCategoryImage),
            IsAndroidBackedUp = ac.IsAndroidBackedUp,
            IsServerBackedUp = ac.IsServerBackedUp,
            CreatedDate = ac.CreatedDate,
            UpdatedDate = ac.UpdatedDate,
            Assets = ac.Assets?.Select(a => new AssetResponseDTO
            {
                AssetServerId = a.AssetServerId,
                AssetLocalId = a.AssetLocalId,
                AssetName = a.AssetName,
                AssetDescription = a.AssetDescription,
                QRCodeSecretText = a.QRCodeSecretText,
                Status = a.Status,
                IsAndroidBackedUp = a.IsAndroidBackedUp,
                IsServerBackedUp = a.IsServerBackedUp,
                CreatedDate = a.CreatedDate,
                UpdatedDate = a.UpdatedDate
            }).ToList() ?? new List<AssetResponseDTO>()
        };

        // ============================================================
        // GET: All Asset Categories
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetAllAssetCategories()
        {
            var categories = await repo.AssetCategory.GetAllAsync();
            var response = categories.Select(MapToDto).ToList();

            return Ok(ApiResponseDTO<List<AssetCategoryResponseDTO>>.SuccessResponse(
                response, "Asset categories retrieved successfully..."));
        }

        // ============================================================
        // GET: All Asset Categories with Assets
        // ============================================================
        [HttpGet("get_all_categories_with_assets")]
        public async Task<IActionResult> GetAllAssetCategoriesWithAssets()
        {
            var categories = await repo.AssetCategory.GetAssetCategoriesWithAssetsAsync();
            var response = categories.Select(MapToDto).ToList();

            return Ok(ApiResponseDTO<List<AssetCategoryResponseDTO>>.SuccessResponse(
                response, "Asset categories with assets retrieved successfully..."));
        }

        // ============================================================
        // GET: Single Category by ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssetCategoryById([FromRoute] int id)
        {
            var category = await repo.AssetCategory.GetByIdAsync(id);
            if (category == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Asset category not found."));

            return Ok(ApiResponseDTO<AssetCategoryResponseDTO>.SuccessResponse(
                MapToDto(category), "Asset category retrieved successfully..."));
        }

        // ============================================================
        // GET: Category by ID with Assets
        // ============================================================
        [HttpGet("get_category_with_assets/{id}")]
        public async Task<IActionResult> GetAssetCategoryByIdWithAssets([FromRoute] int id)
        {
            var category = await repo.AssetCategory.GetAssetCategoryWithAssetsAsync(id);
            if (category == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Asset category not found."));

            return Ok(ApiResponseDTO<AssetCategoryResponseDTO>.SuccessResponse(
                MapToDto(category), "Asset category with assets retrieved successfully..."));
        }

        // ============================================================
        // POST: Create Asset Category
        // ============================================================
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE}")]
        public async Task<IActionResult> CreateAssetCategory([FromForm] CreateAssetCategoryRequestDTO dto)
        {
            var uploadedImage = await SaveImageAsync(dto.ImageFile);

            var newCategory = new AssetCategory
            {
                AssetCategoryLocalId = dto.AssetCategoryLocalId,
                AssetCategoryName = dto.AssetCategoryName,
                AssetCategoryDescription = dto.AssetCategoryDescription,
                AssetCategoryImage = uploadedImage,
                IsAndroidBackedUp = dto.IsAndroidBackedUp,
                IsServerBackedUp = dto.IsServerBackedUp,
                CreatedDate = DateTime.UtcNow
            };

            await repo.AssetCategory.CreateAsync(newCategory);
            await repo.SaveAsync();

            return Ok(ApiResponseDTO<AssetCategoryResponseDTO>.SuccessResponse(
                MapToDto(newCategory), "Asset category created successfully..."));
        }

        // ============================================================
        // PUT: Update Asset Category
        // ============================================================
        [HttpPut]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> UpdateAssetCategory([FromForm] UpdateAssetCategoryRequestDTO dto)
        {
            var category = await repo.AssetCategory.GetByIdAsync(dto.AssetCategoryServerId);
            if (category == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Asset category not found."));

            if (dto.ImageFile != null)
                category.AssetCategoryImage = await SaveImageAsync(dto.ImageFile);

            category.AssetCategoryLocalId = dto.AssetCategoryLocalId ?? category.AssetCategoryLocalId;
            category.AssetCategoryName = string.IsNullOrWhiteSpace(dto.AssetCategoryName) ? category.AssetCategoryName : dto.AssetCategoryName;
            category.AssetCategoryDescription = string.IsNullOrWhiteSpace(dto.AssetCategoryDescription) ? category.AssetCategoryDescription : dto.AssetCategoryDescription;
            category.IsAndroidBackedUp = dto.IsAndroidBackedUp;
            category.IsServerBackedUp = dto.IsServerBackedUp;
            category.UpdatedDate = DateTime.UtcNow;

            await repo.AssetCategory.UpdateAsync(category);
            await repo.SaveAsync();

            return Ok(ApiResponseDTO<AssetCategoryResponseDTO>.SuccessResponse(
                MapToDto(category), "Asset category updated successfully..."));
        }

        // ============================================================
        // DELETE: Asset Category
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> DeleteAssetCategory([FromRoute] int id)
        {
            var category = await repo.AssetCategory.GetByIdAsync(id);
            if (category == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Asset category not found."));

            DeleteImageIfExists(category.AssetCategoryImage);

            await repo.AssetCategory.DeleteAsync(category);
            await repo.SaveAsync();

            return Ok(ApiResponseDTO<string>.SuccessResponse("Asset category deleted successfully"));
        }
    }
}
