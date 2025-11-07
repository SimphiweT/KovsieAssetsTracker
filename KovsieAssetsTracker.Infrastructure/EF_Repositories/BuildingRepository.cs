using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories;
using KovsieAssetsTracker.Infrastructure.EF_Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace KovsieAssetsTracker.Infrastructure.EF_Repositories
{
    /// <summary>
    /// Provides data access functionality for <see cref="Building"/> entities,
    /// supporting eager loading of related Floors and Campus data.
    /// </summary>
    public class BuildingRepository : RepositoryBase<Building>, IBuildingRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingRepository"/> class.
        /// </summary>
        public BuildingRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<List<Building>> GetBuildingsWithFloorsAsync()
        {
            return await appDbContext.Buildings
                .AsNoTracking()
                .Include(b => b.Floors)
                .Include(b => b.Campus)
                .OrderBy(b => b.BuildingName)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Building?> GetBuildingWithFloorsAsync(int buildingId)
        {
            return await appDbContext.Buildings
                .AsNoTracking()
                .Include(b => b.Floors)
                .Include(b => b.Campus)
                .FirstOrDefaultAsync(b => b.BuildingServerId == buildingId);
        }
    }
}
