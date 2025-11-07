using KovsieAssetsTracker.Application.DTOs.BuildingDTOs;
using KovsieAssetsTracker.Application.DTOs.CampusDTOs;
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
    [Authorize]
    public class CampusesController : ControllerBase
    {
        private readonly IRepositoryWrapper repo;
        private readonly IWebHostEnvironment env;

        private static readonly string folderName = "campuses";
        private static readonly string defaultImage = "default-campus.png";
        public CampusesController(IRepositoryWrapper _repo, IWebHostEnvironment _env)
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
        /// <summary>Get All Campuses</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCampuses()
        {
            var campuses = await repo.Campus.GetAllAsync();

            var response = campuses.Select(c => new CampusResponseDTO
            {
                CampusServerId = c.CampusServerId,
                CampusLocalId = c.CampusLocalId,
                CampusName = c.CampusName,
                CampusDescription = c.CampusDescription,
                CampusImage = GetBaseUrl(folderName) + c.CampusImage,
                IsAndroidBackedUp = c.IsAndroidBackedUp,
                IsServerBackedUp = c.IsServerBackedUp,
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate
            }).ToList();

            return Ok(new ApiResponseDTO<List<CampusResponseDTO>>
            {
                Success = true,
                Message ="Campuses retrieved successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Get All Campuses With Buildings </summary>
        [HttpGet("get_all_campuses_with_buildings")]
        public async Task<IActionResult> GetAllCampusesWithBuildings()
        {
            var campuses = await repo.Campus.GetCampusesWithBuildingsAsync();

            var response = campuses.Select(c => new CampusResponseDTO
            {
                CampusServerId = c.CampusServerId,
                CampusName = c.CampusName,
                CampusDescription = c.CampusDescription,
                CampusImage = GetBaseUrl(folderName) + c.CampusImage,
                IsAndroidBackedUp = c.IsAndroidBackedUp,
                IsServerBackedUp = c.IsServerBackedUp,
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate,
                Buildings = c.Buildings?.Select(b => new BuildingResponseDTO
                {
                    BuildingServerId = b.BuildingServerId,
                    BuildingLocalId = b.BuildingLocalId,
                    BuildingName = b.BuildingName,
                    BuildingDescription = b.BuildingDescription,
                    IsAndroidBackedUp = b.IsAndroidBackedUp,
                    IsServerBackedUp = b.IsServerBackedUp,
                    CreatedDate = b.CreatedDate,
                    UpdatedDate = b.UpdatedDate
                }).ToList() ?? new List<BuildingResponseDTO> { }
            }).ToList();

            return Ok(new ApiResponseDTO<List<CampusResponseDTO>>
            {
                Success = true,
                Message = "Campuses retrieved successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Get Campus By Id</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCampusById([FromRoute]int id)
        {
            var campus = await repo.Campus.GetByIdAsync(id);
            if (campus == null)
            {
                return NotFound(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = null,
                    Error = "Campus not found.",
                    Data = null
                });
            }

            var response = new CampusResponseDTO
            {
                CampusServerId = campus.CampusServerId,
                CampusName = campus.CampusName,
                CampusDescription = campus.CampusDescription,
                CampusImage = GetBaseUrl(folderName) + campus.CampusImage,
                IsAndroidBackedUp = campus.IsAndroidBackedUp,
                IsServerBackedUp = campus.IsServerBackedUp,
                CreatedDate = campus.CreatedDate,
                UpdatedDate = campus.UpdatedDate
            };


            return Ok(new ApiResponseDTO<CampusResponseDTO>
            {
                Success = true,
                Message = "Campus retrieved successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Get Campus By Id With Buildings</summary>
        [HttpGet("get_campus_with_buildings/{id}")]
        public async Task<IActionResult> GetCampusByIdWithBuildings(int id)
        {
            var campus = await repo.Campus.GetCampusWithBuildingsAsync(id);
            if (campus == null)
            {
                return NotFound(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = null,
                    Error = "Campus not found.",
                    Data = null
                });
            }

            var response = new CampusResponseDTO
            {
                CampusServerId = campus.CampusServerId,
                CampusName = campus.CampusName,
                CampusDescription = campus.CampusDescription,
                CampusImage = GetBaseUrl(folderName) + campus.CampusImage,
                IsAndroidBackedUp = campus.IsAndroidBackedUp,
                IsServerBackedUp = campus.IsServerBackedUp,
                CreatedDate = campus.CreatedDate,
                UpdatedDate = campus.UpdatedDate,
                Buildings = campus.Buildings?.Select(b => new BuildingResponseDTO
                {
                    BuildingServerId = b.BuildingServerId,
                    BuildingLocalId = b.BuildingLocalId,
                    BuildingName = b.BuildingName,
                    BuildingDescription = b.BuildingDescription,
                    BuildingImage = GetBaseUrl(folderName) + b.BuildingImage,
                    IsAndroidBackedUp = b.IsAndroidBackedUp,
                    IsServerBackedUp = b.IsServerBackedUp,
                    CreatedDate = b.CreatedDate,
                    UpdatedDate = b.UpdatedDate
                }).ToList()
            };


            return Ok(new ApiResponseDTO<CampusResponseDTO>
            {
                Success = true,
                Message = "Campus retrieved successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Create a new campus</summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> CreateCampus([FromForm] CreateCampusRequestDTO dto)
        {


                var uploadedImage = await SaveImageAsync(dto.ImageFile);


            var campus = new Campus
            {
                CampusLocalId = dto.CampusLocalId,
                CampusName = dto.CampusName,
                CampusDescription = dto.CampusDescription,
                CampusImage = uploadedImage,
                IsAndroidBackedUp = dto.IsAndroidBackedUp,
                IsServerBackedUp = dto.IsServerBackedUp,
                CreatedDate = DateTime.ParseExact(DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm"), "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture),
            };

            var newCampus = await repo.Campus.CreateAsync(campus);
            await repo.SaveAsync();

            var response = new CampusResponseDTO
            {
                CampusServerId = newCampus.CampusServerId,
                CampusLocalId = newCampus.CampusLocalId,
                CampusName = newCampus.CampusName,
                CampusDescription = newCampus.CampusDescription,
                IsAndroidBackedUp = newCampus.IsAndroidBackedUp,
                IsServerBackedUp = newCampus.IsServerBackedUp,
                CreatedDate = newCampus.CreatedDate,
                UpdatedDate = newCampus.UpdatedDate
            };

            return Ok(new ApiResponseDTO<CampusResponseDTO>
            {
                Success = true,
                Message = "Campus created successfully...",
                Error = null,
                Data = response
            });
        }

        /// <summary>Update a campus</summary>
        [HttpPut]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> UpdateCampus([FromForm] UpdateCampusRequestDTO dto)
        {
            var campus = await repo.Campus.GetByIdAsync(dto.CampusServerId);
            if (campus == null)
            {
                return NotFound(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = null,
                    Error = "Campus not found.",
                    Data = null
                });
            }

            if (dto.ImageFile != null)
            {
                var uploadedImage = await SaveImageAsync(dto.ImageFile);
                campus.CampusImage = uploadedImage;
            }

            campus.CampusLocalId = dto.CampusLocalId ?? campus.CampusLocalId;
            campus.CampusName = string.IsNullOrWhiteSpace(dto.CampusName) ? campus.CampusName : dto.CampusName;
            campus.CampusDescription = string.IsNullOrWhiteSpace(dto.CampusDescription) ? campus.CampusDescription : dto.CampusDescription;
            campus.IsAndroidBackedUp = dto.IsAndroidBackedUp;
            campus.IsServerBackedUp = dto.IsServerBackedUp;
            campus.UpdatedDate = DateTime.ParseExact(DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm"), "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);

            await repo.Campus.UpdateAsync(campus);
            await repo.SaveAsync();

            var response = new CampusResponseDTO
            {
                CampusServerId = campus.CampusServerId,
                CampusLocalId = campus.CampusLocalId,
                CampusName = campus.CampusName,
                CampusDescription = campus.CampusDescription,
                CampusImage = GetBaseUrl(folderName) + campus.CampusImage,
                IsAndroidBackedUp = campus.IsAndroidBackedUp,
                IsServerBackedUp = campus.IsServerBackedUp,
                CreatedDate = campus.CreatedDate,
                UpdatedDate = campus.UpdatedDate
            };

            return Ok(new ApiResponseDTO<CampusResponseDTO>
            {
                Success = true,
                Message = "Campus updated successfully...",
                Error = null,
                Data = response
            });
        }


        /// <summary>Delete a campus</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> Delete(int id)
        {
            var campus = await repo.Campus.GetByIdAsync(id);
            if (campus == null)
            {
                return NotFound(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = null,
                    Error = "Campus not found.",
                    Data = null
                });
            }


            if (!string.IsNullOrEmpty(campus.CampusImage) && campus.CampusImage != defaultImage)
            {
                var filePath = Path.Combine(folderName, campus.CampusImage);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await repo.Campus.DeleteAsync(campus);
            await repo.SaveAsync();

            return Ok(new ApiResponseDTO<string>
            {
                Success = true,
                Message = "Campus deleted successfully",
                Error= null,
                Data=null
            });
        }


    }
}
