using KovsieAssetsTracker.Application.DTOs.BuildingDTOs;
using KovsieAssetsTracker.Application.DTOs.FloorDTOs;
using KovsieAssetsTracker.Application.DTOs.RoomDTOs;
using KovsieAssetsTracker.Application.DTOs.SharedDTOs;
using KovsieAssetsTracker.Application.Helpers;
using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace KovsieAssetsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FloorsController : ControllerBase
    {
        private readonly IRepositoryWrapper repo;

        public FloorsController(IRepositoryWrapper _repo)
        {
            repo = _repo;
        }

        /// <summary>Retrieves all floors.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllFloors()
        {
            var floors = await repo.Floor.GetAllAsync();

            var response = floors.Select(f => new FloorResponseDTO
            {
                FloorServerId = f.FloorServerId,
                FloorLocalId = f.FloorLocalId,
                FloorName = f.FloorName,
                FloorDescription = f.FloorDescription,
                IsAndroidBackedUp = f.IsAndroidBackedUp,
                IsServerBackedUp = f.IsServerBackedUp,
                CreatedDate = f.CreatedDate,
                UpdatedDate = f.UpdatedDate,
                BuildingServerId = f.BuildingServerId,
                BuildingName = f.Building?.BuildingName
            }).ToList();

            return Ok(new ApiResponseDTO<List<FloorResponseDTO>>
            {
                Success = true,
                Message = "Floors retrieved successfully...",
                Data = response
            });
        }

        /// <summary>Retrieves all floors with their associated rooms.</summary>
        [HttpGet("get_all_floors_with_rooms")]
        public async Task<IActionResult> GetAllFloorsWithRooms()
        {
            var floors = await repo.Floor.GetFloorsWithRoomsAsync();

            var response = floors.Select(f => new FloorResponseDTO
            {
                FloorServerId = f.FloorServerId,
                FloorLocalId = f.FloorLocalId,
                FloorName = f.FloorName,
                FloorDescription = f.FloorDescription,
                IsAndroidBackedUp = f.IsAndroidBackedUp,
                IsServerBackedUp = f.IsServerBackedUp,
                CreatedDate = f.CreatedDate,
                UpdatedDate = f.UpdatedDate,
                BuildingServerId = f.BuildingServerId,
                BuildingName = f.Building?.BuildingName,
                Rooms = f.Rooms?.Select(r => new RoomResponseDTO
                {
                    RoomServerId = r.RoomServerId,
                    RoomLocalId = r.RoomLocalId,
                    RoomName = r.RoomName,
                    RoomDescription = r.RoomDescription,
                    RoomImage = r.RoomImage,
                    IsAndroidBackedUp = r.IsAndroidBackedUp,
                    IsServerBackedUp = r.IsServerBackedUp,
                    CreatedDate = r.CreatedDate,
                    UpdatedDate = r.UpdatedDate
                }).ToList()
            }).ToList();

            return Ok(new ApiResponseDTO<List<FloorResponseDTO>>
            {
                Success = true,
                Message = "Floors with rooms retrieved successfully...",
                Data = response
            });
        }

        /// <summary>Retrieves a floor by its ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFloorById([FromRoute] int id)
        {
            var floor = await repo.Floor.GetByIdAsync(id);
            if (floor == null)
                return NotFound(new ApiResponseDTO<string> { Success = false, Error = "Floor not found." });

            var response = new FloorResponseDTO
            {
                FloorServerId = floor.FloorServerId,
                FloorLocalId = floor.FloorLocalId,
                FloorName = floor.FloorName,
                FloorDescription = floor.FloorDescription,
                IsAndroidBackedUp = floor.IsAndroidBackedUp,
                IsServerBackedUp = floor.IsServerBackedUp,
                CreatedDate = floor.CreatedDate,
                UpdatedDate = floor.UpdatedDate,
                BuildingServerId = floor.BuildingServerId,
                BuildingName = floor.Building?.BuildingName
            };

            return Ok(new ApiResponseDTO<FloorResponseDTO>
            {
                Success = true,
                Message = "Floor retrieved successfully...",
                Data = response
            });
        }

        /// <summary>Retrieves a floor by ID with its rooms.</summary>
        [HttpGet("get_floor_with_rooms/{id}")]
        public async Task<IActionResult> GetFloorByIdWithRooms(int id)
        {
            var floor = await repo.Floor.GetFloorWithRoomsAsync(id);
            if (floor == null)
                return NotFound(new ApiResponseDTO<string> { Success = false, Error = "Floor not found." });

            var response = new FloorResponseDTO
            {
                FloorServerId = floor.FloorServerId,
                FloorLocalId = floor.FloorLocalId,
                FloorName = floor.FloorName,
                FloorDescription = floor.FloorDescription,
                IsAndroidBackedUp = floor.IsAndroidBackedUp,
                IsServerBackedUp = floor.IsServerBackedUp,
                CreatedDate = floor.CreatedDate,
                UpdatedDate = floor.UpdatedDate,
                BuildingServerId = floor.BuildingServerId,
                BuildingName = floor.Building?.BuildingName,
                Rooms = floor.Rooms?.Select(r => new RoomResponseDTO
                {
                    RoomServerId = r.RoomServerId,
                    RoomLocalId = r.RoomLocalId,
                    RoomName = r.RoomName,
                    RoomDescription = r.RoomDescription,
                    RoomImage = r.RoomImage,
                    IsAndroidBackedUp = r.IsAndroidBackedUp,
                    IsServerBackedUp = r.IsServerBackedUp,
                    CreatedDate = r.CreatedDate,
                    UpdatedDate = r.UpdatedDate
                }).ToList()
            };

            return Ok(new ApiResponseDTO<FloorResponseDTO>
            {
                Success = true,
                Message = "Floor retrieved successfully...",
                Data = response
            });
        }

        /// <summary>Creates a new floor.</summary>
        [HttpPost]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> CreateFloor([FromBody] CreateFloorRequestDTO dto)
        {
            var floor = new Floor
            {
                FloorLocalId = dto.FloorLocalId,
                FloorName = dto.FloorName,
                FloorDescription = dto.FloorDescription,
                IsAndroidBackedUp = dto.IsAndroidBackedUp,
                IsServerBackedUp = dto.IsServerBackedUp,
                CreatedDate = DateTime.UtcNow,
                BuildingServerId = dto.BuildingServerId
            };

            var newFloor = await repo.Floor.CreateAsync(floor);
            await repo.SaveAsync();

            var response = new FloorResponseDTO
            {
                FloorServerId = newFloor.FloorServerId,
                FloorLocalId = newFloor.FloorLocalId,
                FloorName = newFloor.FloorName,
                FloorDescription = newFloor.FloorDescription,
                CreatedDate = newFloor.CreatedDate,
                UpdatedDate = newFloor.UpdatedDate
            };

            return Ok(new ApiResponseDTO<FloorResponseDTO>
            {
                Success = true,
                Message = "Floor created successfully...",
                Data = response
            });
        }

        /// <summary>Updates an existing floor.</summary>
        [HttpPut]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> UpdateFloor([FromBody] UpdateFloorRequestDTO dto)
        {
            var floor = await repo.Floor.GetByIdAsync(dto.FloorServerId);
            if (floor == null)
                return NotFound(new ApiResponseDTO<string> { Success = false, Error = "Floor not found." });

            floor.FloorName = string.IsNullOrWhiteSpace(dto.FloorName) ? floor.FloorName : dto.FloorName;
            floor.FloorDescription = string.IsNullOrWhiteSpace(dto.FloorDescription) ? floor.FloorDescription : dto.FloorDescription;
            floor.FloorLocalId = dto.FloorLocalId ?? floor.FloorLocalId;
            floor.IsAndroidBackedUp = dto.IsAndroidBackedUp;
            floor.IsServerBackedUp = dto.IsServerBackedUp;
            floor.BuildingServerId = dto.BuildingServerId ?? floor.BuildingServerId;
            floor.UpdatedDate = DateTime.UtcNow;

            await repo.Floor.UpdateAsync(floor);
            await repo.SaveAsync();

            return Ok(new ApiResponseDTO<string>
            {
                Success = true,
                Message = "Floor updated successfully"
            });
        }

        /// <summary>Deletes a floor.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            var floor = await repo.Floor.GetByIdAsync(id);
            if (floor == null)
                return NotFound(new ApiResponseDTO<string> { Success = false, Error = "Floor not found." });

            await repo.Floor.DeleteAsync(floor);
            await repo.SaveAsync();

            return Ok(new ApiResponseDTO<string>
            {
                Success = true,
                Message = "Floor deleted successfully"
            });
        }
    }
}
