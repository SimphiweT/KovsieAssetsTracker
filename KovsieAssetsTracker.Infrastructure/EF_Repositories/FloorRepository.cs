using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories;
using KovsieAssetsTracker.Infrastructure.EF_Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace KovsieAssetsTracker.Infrastructure.EF_Repositories
{
    /// <summary>
    /// Provides data access functionality for <see cref="Floor"/> entities,
    /// supporting eager loading of related Rooms and Buildings.
    /// </summary>
    public class FloorRepository : RepositoryBase<Floor>, IFloorRepository
    {
        public FloorRepository(AppDbContext context) : base(context) { }

        /// <inheritdoc />
        public async Task<List<Floor>> GetFloorsWithRoomsAsync()
        {
            return await appDbContext.Floors
                .AsNoTracking()
                .Include(f => f.Rooms)
                .Include(f => f.Building)
                .OrderBy(f => f.FloorName)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Floor?> GetFloorWithRoomsAsync(int floorId)
        {
            return await appDbContext.Floors
                .AsNoTracking()
                .Include(f => f.Rooms)
                .Include(f => f.Building)
                .FirstOrDefaultAsync(f => f.FloorServerId == floorId);
        }
    }
}
