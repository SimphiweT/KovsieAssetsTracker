using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;

namespace KovsieAssetsTracker.Domain.IRepositories
{
    public interface IAssetCategoryRepository : IRepositoryBase<AssetCategory>
    {
        /// <summary>
        /// Retrieves all asset categories including their related assets.
        /// </summary>
        Task<List<AssetCategory>> GetAssetCategoriesWithAssetsAsync();

        /// <summary>
        /// Retrieves a single asset category with its related assets.
        /// </summary>
        Task<AssetCategory?> GetAssetCategoryWithAssetsAsync(int categoryId);
    }
}
