using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;

namespace KovsieAssetsTracker.Domain.IRepositories
{
    /// <summary>
    /// Defines data access operations for <see cref="Floor"/> entities,
    /// including their related Rooms and Buildings.
    /// </summary>
    public interface IFloorRepository : IRepositoryBase<Floor>
    {
        /// <summary>
        /// Retrieves all floors including their associated rooms.
        /// </summary>
        Task<List<Floor>> GetFloorsWithRoomsAsync();

        /// <summary>
        /// Retrieves a single floor including its associated rooms.
        /// </summary>
        /// <param name="floorId">The unique ID of the floor.</param>
        Task<Floor?> GetFloorWithRoomsAsync(int floorId);
    }
}
