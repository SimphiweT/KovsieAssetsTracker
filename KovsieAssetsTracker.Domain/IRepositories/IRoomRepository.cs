using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;

namespace KovsieAssetsTracker.Domain.IRepositories
{
    /// <summary>
    /// Defines the contract for room data access including floors and assets.
    /// </summary>
    public interface IRoomRepository : IRepositoryBase<Room>
    {
        /// <summary>
        /// Retrieves all rooms with their associated assets.
        /// </summary>
        Task<List<Room>> GetAllRoomsWithAssetsAsync();

        /// <summary>
        /// Retrieves all rooms with both floor and asset relationships.
        /// </summary>
        Task<List<Room>> GetAllRoomsWithFloorAndAssetsAsync();

        /// <summary>
        /// Retrieves a single room including its assets.
        /// </summary>
        Task<Room?> GetRoomWithAssetsAsync(int roomServerId);

        /// <summary>
        /// Retrieves all rooms including only floor details.
        /// </summary>
        Task<List<Room>> GetRoomsWithFloorOnlyAsync();
    }
}
