using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;

namespace KovsieAssetsTracker.Domain.IRepositories
{
    /// <summary>
    /// Defines data access operations for <see cref="Campus"/> entities,
    /// including related buildings.
    /// </summary>
    public interface ICampusRepository : IRepositoryBase<Campus>
    {
        /// <summary>
        /// Retrieves all campuses including their associated buildings.
        /// </summary>
        Task<List<Campus>> GetCampusesWithBuildingsAsync();

        /// <summary>
        /// Retrieves a single campus including its associated buildings.
        /// </summary>
        /// <param name="campusId">The unique ID of the campus.</param>
        Task<Campus?> GetCampusWithBuildingsAsync(int campusId);
    }
}
