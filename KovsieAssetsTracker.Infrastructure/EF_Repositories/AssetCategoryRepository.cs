using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories;
using KovsieAssetsTracker.Infrastructure.EF_Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace KovsieAssetsTracker.Infrastructure.EF_Repositories
{
    public class AssetCategoryRepository : RepositoryBase<AssetCategory>, IAssetCategoryRepository
    {
        public AssetCategoryRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all asset categories including related assets.
        /// </summary>
        public async Task<List<AssetCategory>> GetAssetCategoriesWithAssetsAsync()
        {
            return await appDbContext.AssetCategories
                .AsNoTracking()
                .Include(ac => ac.Assets)
                .OrderBy(ac => ac.AssetCategoryName)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single asset category with its related assets by ID.
        /// </summary>
        public async Task<AssetCategory?> GetAssetCategoryWithAssetsAsync(int categoryId)
        {
            return await appDbContext.AssetCategories
                .AsNoTracking()
                .Include(ac => ac.Assets)
                .FirstOrDefaultAsync(ac => ac.AssetCategoryServerId == categoryId);
        }
    }
}
