using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;

namespace KovsieAssetsTracker.Domain.IRepositories
{
    /// <summary>
    /// Defines data access operations for <see cref="Building"/> entities,
    /// including their related Floors and Campus.
    /// </summary>
    public interface IBuildingRepository : IRepositoryBase<Building>
    {
        /// <summary>
        /// Retrieves all buildings including their associated floors.
        /// </summary>
        Task<List<Building>> GetBuildingsWithFloorsAsync();

        /// <summary>
        /// Retrieves a single building including its associated floors.
        /// </summary>
        /// <param name="buildingId">The unique ID of the building.</param>
        Task<Building?> GetBuildingWithFloorsAsync(int buildingId);
    }
}
