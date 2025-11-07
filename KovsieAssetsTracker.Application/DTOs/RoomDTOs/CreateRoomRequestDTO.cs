using Microsoft.AspNetCore.Http;

namespace KovsieAssetsTracker.Application.DTOs.RoomDTOs
{
    public class CreateRoomRequestDTO
    {
        public int? RoomLocalId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string RoomDescription { get; set; } = string.Empty;
        public string? RoomImage { get; set; }
        public bool IsAndroidBackedUp { get; set; } = false;
        public bool IsServerBackedUp { get; set; } = false;
        public int? FloorServerId { get; set; }
        public IFormFile? ImageFile { get; set; }

    }
}
