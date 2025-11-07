using KovsieAssetsTracker.Application.DTOs.BuildingDTOs;
using KovsieAssetsTracker.Application.DTOs.CampusDTOs;
using KovsieAssetsTracker.Application.DTOs.FloorDTOs;
using KovsieAssetsTracker.Application.DTOs.SharedDTOs;
using KovsieAssetsTracker.Application.Helpers;
using KovsieAssetsTracker.Domain.IRepositories.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace KovsieAssetsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingsController : ControllerBase
    {
        private readonly IRepositoryWrapper repo;
        private readonly IWebHostEnvironment env;

        private static readonly string folderName = "buildings";
        private static readonly string defaultImage = "default-building.png";
        public BuildingsController(IRepositoryWrapper _repo, IWebHostEnvironment _env)
        {
            repo = _repo;
            env = _env;
        }
        private string GetBaseUrl(string folder)
        {
            return $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/{folder}/";
        }
        private async Task<string> SaveImageAsync(IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                var folder = Path.Combine(env.WebRootPath, "images", folderName);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                return fileName;
            }

            return defaultImage;
        }

        /// <summary>Get All buildings</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllBuildings()
        {
            var buildings = await repo.Building.GetAllAsync();

            var response = buildings.Select(b => new BuildingResponseDTO
            {
                BuildingServerId = b.BuildingServerId,
                BuildingLocalId = b.BuildingLocalId,
                BuildingName = b.BuildingName,
                BuildingDescription = b.BuildingDescription,
                BuildingImage = GetBaseUrl(folderName) + b.BuildingImage,
                IsAndroidBackedUp = b.IsAndroidBackedUp,
                IsServerBackedUp = b.IsServerBackedUp,
                CreatedDate = b.CreatedDate,
                UpdatedDate = b.UpdatedDate,
                CampusServerId = b.CampusServerId,
                CampusName = b.Campus?.CampusName

            }).ToList();

            return Ok(new ApiResponseDTO<List<BuildingResponseDTO>>
            {
                Success = true,
                Message = "Buildings retrieved successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Get All Buildings With Floors </summary>
        [HttpGet("get_all_buildings_with_floors")]
        public async Task<IActionResult> GetAllBuildingsWithFloors()
        {
            var buildings = await repo.Building.GetAllAsync();

            var response = buildings.Select(b => new BuildingResponseDTO
            {
                BuildingServerId = b.BuildingServerId,
                BuildingLocalId = b.BuildingLocalId,
                BuildingName = b.BuildingName,
                BuildingDescription = b.BuildingDescription,
                BuildingImage = GetBaseUrl(folderName) + b.BuildingImage,
                IsAndroidBackedUp = b.IsAndroidBackedUp,
                IsServerBackedUp = b.IsServerBackedUp,
                CreatedDate = b.CreatedDate,
                UpdatedDate = b.UpdatedDate,
                CampusServerId = b.CampusServerId,
                CampusName = b.Campus?.CampusName,
                Floors = b.Floors?.Select(f => new FloorResponseDTO
                {
                    FloorServerId = f.FloorServerId,
                    FloorLocalId = f.FloorLocalId,
                    FloorName = f.FloorName,
                    FloorDescription = f.FloorDescription,
                    IsAndroidBackedUp = f.IsAndroidBackedUp,
                    IsServerBackedUp = f.IsServerBackedUp,
                    CreatedDate = f.CreatedDate,
                    UpdatedDate = f.UpdatedDate
                }).ToList() ?? new List<FloorResponseDTO> { }
            }).ToList();

            return Ok(new ApiResponseDTO<List<BuildingResponseDTO>>
            {
                Success = true,
                Message = "Buildings retrieved successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Get Building By Id</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBuildingById([FromRoute] int id)
        {
            var building = await repo.Building.GetByIdAsync(id);
            if (building == null)
            {
                return NotFound(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = null,
                    Error = "Building not found.",
                    Data = null
                });
            }

            var response = new BuildingResponseDTO
            {
                BuildingServerId = building.BuildingServerId,
                BuildingLocalId = building.BuildingLocalId,
                BuildingName = building.BuildingName,
                BuildingDescription = building.BuildingDescription,
                BuildingImage = GetBaseUrl(folderName) + building.BuildingImage,
                IsAndroidBackedUp = building.IsAndroidBackedUp,
                IsServerBackedUp = building.IsServerBackedUp,
                CreatedDate = building.CreatedDate,
                UpdatedDate = building.UpdatedDate,
                CampusServerId = building.CampusServerId,
                CampusName = building.Campus?.CampusName

            };


            return Ok(new ApiResponseDTO<BuildingResponseDTO>
            {
                Success = true,
                Message = "Building retrieved successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Get Building By Id With Floors</summary>
        [HttpGet("get_building_with_floors/{id}")]
        public async Task<IActionResult> GetBuildingByIdWithFloors([FromRoute] int id)
        {
            var building = await repo.Building.GetBuildingWithFloorsAsync(id);
            if (building == null)
            {
                return NotFound(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = null,
                    Error = "Building not found.",
                    Data = null
                });
            }

            var response = new BuildingResponseDTO
            {
                BuildingServerId = building.BuildingServerId,
                BuildingLocalId = building.BuildingLocalId,
                BuildingName = building.BuildingName,
                BuildingDescription = building.BuildingDescription,
                BuildingImage = GetBaseUrl(folderName) + building.BuildingImage,
                IsAndroidBackedUp = building.IsAndroidBackedUp,
                IsServerBackedUp = building.IsServerBackedUp,
                CreatedDate = building.CreatedDate,
                UpdatedDate = building.UpdatedDate,
                CampusServerId = building.CampusServerId,
                CampusName = building.Campus?.CampusName,
                Floors = building.Floors?.Select(f => new FloorResponseDTO
                {
                    FloorServerId = f.FloorServerId,
                    FloorLocalId = f.FloorLocalId,
                    FloorName = f.FloorName,
                    FloorDescription = f.FloorDescription,
                    IsAndroidBackedUp = f.IsAndroidBackedUp,
                    IsServerBackedUp = f.IsServerBackedUp,
                    CreatedDate = f.CreatedDate,
                    UpdatedDate = f.UpdatedDate
                }).ToList() ?? new List<FloorResponseDTO> { }
            };


            return Ok(new ApiResponseDTO<BuildingResponseDTO>
            {
                Success = true,
                Message = "Building retrieved successfully...",
                Error = null,
                Data = response
            });
        }


        /// <summary>Create a new building</summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> CreateBuilding([FromForm] CreateBuildingRequestDTO dto)
        {
            var uploadedImage = await SaveImageAsync(dto.ImageFile);

            var building = new Building
            {
                BuildingLocalId = dto.BuildingLocalId,
                BuildingName = dto.BuildingName,
                BuildingDescription = dto.BuildingDescription,
                BuildingImage = uploadedImage,
                IsAndroidBackedUp = dto.IsAndroidBackedUp,
                IsServerBackedUp = dto.IsServerBackedUp,
                CreatedDate = DateTime.ParseExact(DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm"), "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture),
                CampusServerId = dto.CampusServerId
            };

            var newBuilding = await repo.Building.CreateAsync(building);
            await repo.SaveAsync();

            var response = new BuildingResponseDTO
            {
                BuildingServerId = newBuilding.BuildingServerId,
                BuildingLocalId = newBuilding.BuildingLocalId,
                BuildingName = newBuilding.BuildingName,
                BuildingDescription = newBuilding.BuildingDescription,
                BuildingImage = GetBaseUrl(folderName) + newBuilding.BuildingImage,
                IsAndroidBackedUp = newBuilding.IsAndroidBackedUp,
                IsServerBackedUp = newBuilding.IsServerBackedUp,
                CreatedDate = newBuilding.CreatedDate,
                UpdatedDate = newBuilding.UpdatedDate,
                CampusServerId = dto.CampusServerId
            };

            return Ok(new ApiResponseDTO<BuildingResponseDTO>
            {
                Success = true,
                Message = "Building created successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Update a building</summary>
        [HttpPut]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> UpdateBuilding([FromForm] UpdateBuildingRequestDTO dto)
        {
            var building = await repo.Building.GetByIdAsync(dto.BuildingServerId);
            if (building == null)
            {
                return NotFound(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = null,
                    Error = "Building not found.",
                    Data = null
                });
            }

            if (dto.ImageFile != null)
            {
                var uploadedImage = await SaveImageAsync(dto.ImageFile);
                building.BuildingImage = uploadedImage;
            }

            building.BuildingLocalId = dto.BuildingLocalId ?? building.BuildingLocalId;
            building.BuildingName = string.IsNullOrWhiteSpace(dto.BuildingName) ? building.BuildingName : dto.BuildingName;
            building.BuildingDescription = string.IsNullOrWhiteSpace(dto.BuildingDescription) ? building.BuildingDescription : dto.BuildingDescription;
            building.IsAndroidBackedUp = dto.IsAndroidBackedUp;
            building.IsServerBackedUp = dto.IsServerBackedUp;
            building.UpdatedDate = DateTime.ParseExact(DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm"), "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
            building.CampusServerId = dto.CampusServerId ?? building.CampusServerId;

            await repo.Building.UpdateAsync(building);
            await repo.SaveAsync();

            var response = new BuildingResponseDTO
            {
                BuildingServerId = building.BuildingServerId,
                BuildingLocalId = building.BuildingLocalId,
                BuildingName = building.BuildingName,
                BuildingDescription = building.BuildingDescription,
                BuildingImage = GetBaseUrl(folderName) + building.BuildingImage,
                IsAndroidBackedUp = building.IsAndroidBackedUp,
                IsServerBackedUp = building.IsServerBackedUp,
                CreatedDate = building.CreatedDate,
                UpdatedDate = building.UpdatedDate,
                CampusServerId = building.CampusServerId
            };

            return Ok(new ApiResponseDTO<BuildingResponseDTO>
            {
                Success = true,
                Message = "Building updated successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Delete a building</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> DeleteBuilding([FromRoute] int id)
        {
            var building = await repo.Building.GetByIdAsync(id);
            if (building == null)
            {
                return NotFound(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = null,
                    Error = "Building not found.",
                    Data = null
                });
            }


            if (!string.IsNullOrEmpty(building.BuildingImage) && building.BuildingImage != defaultImage)
            {
                var filePath = Path.Combine(folderName, building.BuildingImage);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await repo.Building.DeleteAsync(building);
            await repo.SaveAsync();

            return Ok(new ApiResponseDTO<string>
            {
                Success = true,
                Message = "Building deleted successfully",
                Error = null,
                Data = null
            });
        }
    }
}
