using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories;
using KovsieAssetsTracker.Infrastructure.EF_Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace KovsieAssetsTracker.Infrastructure.EF_Repositories
{
    /// <summary>
    /// Provides data access for <see cref="Campus"/> entities,
    /// supporting eager loading of related buildings.
    /// </summary>
    public class CampusRepository : RepositoryBase<Campus>, ICampusRepository
    {
        public CampusRepository(AppDbContext context) : base(context) { }

        /// <inheritdoc />
        public async Task<List<Campus>> GetCampusesWithBuildingsAsync()
        {
            return await appDbContext.Campuses
                .AsNoTracking()
                .Include(c => c.Buildings)
                .OrderBy(c => c.CampusName)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Campus?> GetCampusWithBuildingsAsync(int campusId)
        {
            return await appDbContext.Campuses
                .AsNoTracking()
                .Include(c => c.Buildings)
                .FirstOrDefaultAsync(c => c.CampusServerId == campusId);
        }
    }
}
