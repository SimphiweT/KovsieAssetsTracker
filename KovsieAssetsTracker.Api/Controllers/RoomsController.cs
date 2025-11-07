using KovsieAssetsTracker.Application.DTOs.AssetDTOs;
using KovsieAssetsTracker.Application.DTOs.RoomDTOs;
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
    public class RoomsController : ControllerBase
    {
        private readonly IRepositoryWrapper repo;
        private readonly IWebHostEnvironment env;

        private static readonly string folderName = "rooms";
        private static readonly string defaultImage = "default-room.jpg";

        public RoomsController(IRepositoryWrapper _repo, IWebHostEnvironment _env)
        {
            repo = _repo;
            env = _env;
        }

        private string GetBaseUrl(string folder) =>
            $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/{folder}/";

        private async Task<string> SaveImageAsync(IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                var folder = Path.Combine(env.WebRootPath, "images", folderName);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var path = Path.Combine(folder, fileName);

                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                return fileName;
            }

            return defaultImage;
        }

        // ============================================================
        // GET ALL ROOMS (with Floor + Assets)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await repo.Room.GetAllRoomsWithFloorAndAssetsAsync();

            var response = rooms.Select(MapRoomToDTO).ToList();
            return Ok(ApiResponseDTO<List<RoomResponseDTO>>.SuccessResponse(response, "Rooms retrieved successfully..."));
        }

        // ============================================================
        // GET ROOM BY ID (with Floor + Assets)
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById([FromRoute] int id)
        {
            var room = await repo.Room.GetRoomWithAssetsAsync(id);
            if (room == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Room not found."));

            var response = MapRoomToDTO(room);
            return Ok(ApiResponseDTO<RoomResponseDTO>.SuccessResponse(response, "Room retrieved successfully..."));
        }

        // ============================================================
        // CREATE ROOM
        // ============================================================
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE}")]
        public async Task<IActionResult> CreateRoom([FromForm] CreateRoomRequestDTO dto)
        {
            var uploadedImage = await SaveImageAsync(dto.ImageFile);

            var room = new Room
            {
                RoomLocalId = dto.RoomLocalId,
                RoomName = dto.RoomName,
                RoomDescription = dto.RoomDescription,
                RoomImage = uploadedImage,
                IsAndroidBackedUp = dto.IsAndroidBackedUp,
                IsServerBackedUp = dto.IsServerBackedUp,
                FloorServerId = dto.FloorServerId,
                CreatedDate = DateTime.UtcNow
            };

            await repo.Room.CreateAsync(room);
            await repo.SaveAsync();

            var response = MapRoomToDTO(room);
            return Ok(ApiResponseDTO<RoomResponseDTO>.SuccessResponse(response, "Room created successfully..."));
        }

        // ============================================================
        // UPDATE ROOM
        // ============================================================
        [HttpPut]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> UpdateRoom([FromForm] UpdateRoomRequestDTO dto)
        {
            var room = await repo.Room.GetByIdAsync(dto.RoomServerId);
            if (room == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Room not found."));

            if (dto.ImageFile != null)
                room.RoomImage = await SaveImageAsync(dto.ImageFile);

            room.RoomLocalId = dto.RoomLocalId ?? room.RoomLocalId;
            room.RoomName = string.IsNullOrWhiteSpace(dto.RoomName) ? room.RoomName : dto.RoomName;
            room.RoomDescription = string.IsNullOrWhiteSpace(dto.RoomDescription) ? room.RoomDescription : dto.RoomDescription;
            room.IsAndroidBackedUp = dto.IsAndroidBackedUp;
            room.IsServerBackedUp = dto.IsServerBackedUp;
            room.FloorServerId = dto.FloorServerId ?? room.FloorServerId;
            room.UpdatedDate = DateTime.UtcNow;

            await repo.Room.UpdateAsync(room);
            await repo.SaveAsync();

            var response = MapRoomToDTO(room);
            return Ok(ApiResponseDTO<RoomResponseDTO>.SuccessResponse(response, "Room updated successfully..."));
        }

        // ============================================================
        // DELETE ROOM
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> DeleteRoom([FromRoute] int id)
        {
            var room = await repo.Room.GetByIdAsync(id);
            if (room == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Room not found."));

            if (!string.IsNullOrEmpty(room.RoomImage) && room.RoomImage != defaultImage)
            {
                var filePath = Path.Combine(env.WebRootPath, "images", folderName, room.RoomImage);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            await repo.Room.DeleteAsync(room);
            await repo.SaveAsync();

            return Ok(ApiResponseDTO<string>.SuccessResponse("Room deleted successfully"));
        }

        // ============================================================
        // HELPER: Map Entity to DTO
        // ============================================================
        private RoomResponseDTO MapRoomToDTO(Room r)
        {
            return new RoomResponseDTO
            {
                RoomServerId = r.RoomServerId,
                RoomLocalId = r.RoomLocalId,
                RoomName = r.RoomName,
                RoomDescription = r.RoomDescription,
                RoomImage = GetBaseUrl(folderName) + (string.IsNullOrEmpty(r.RoomImage) ? defaultImage : r.RoomImage),
                IsAndroidBackedUp = r.IsAndroidBackedUp,
                IsServerBackedUp = r.IsServerBackedUp,
                CreatedDate = r.CreatedDate,
                UpdatedDate = r.UpdatedDate,
                FloorServerId = r.FloorServerId,
                FloorName = r.Floor?.FloorName ?? "Unassigned",
                Assets = r.Assets?.Select(a => new AssetResponseDTO
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
        }
    }
}
