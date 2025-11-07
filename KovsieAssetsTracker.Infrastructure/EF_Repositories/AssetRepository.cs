using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories;
using KovsieAssetsTracker.Infrastructure.EF_Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace KovsieAssetsTracker.Infrastructure.EF_Repositories
{
    public class AssetRepository : RepositoryBase<Asset>, IAssetRepository
    {
        public AssetRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all assets including their associated rooms.
        /// </summary>
        public async Task<List<Asset>> GetAllAssetsWithRoomAsync()
        {
            return await appDbContext.Assets
                .AsNoTracking()
                .Include(a => a.Room)
                .OrderBy(a => a.AssetName)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all assets including their associated categories.
        /// </summary>
        public async Task<List<Asset>> GetAllAssetsWithCategoryAsync()
        {
            return await appDbContext.Assets
                .AsNoTracking()
                .Include(a => a.AssetCategory)
                .OrderBy(a => a.AssetName)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all assets including both their category and room details.
        /// </summary>
        public async Task<List<Asset>> GetAllAssetsWithCategoryAndRoomAsync()
        {
            return await appDbContext.Assets
                .AsNoTracking()
                .Include(a => a.AssetCategory)
                .Include(a => a.Room)
                .OrderBy(a => a.AssetName)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all assets located in a specific room.
        /// </summary>
        public async Task<List<Asset>> GetAllAssetsInRoomAsync(int roomServerId)
        {
            return await appDbContext.Assets
                .AsNoTracking()
                .Where(a => a.RoomServerId == roomServerId)
                .Include(a => a.AssetCategory)
                .OrderBy(a => a.AssetName)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all assets within a specific category.
        /// </summary>
        public async Task<List<Asset>> GetAllAssetsInCategoryAsync(int assetCategoryServerId)
        {
            return await appDbContext.Assets
                .AsNoTracking()
                .Where(a => a.AssetCategoryServerId == assetCategoryServerId)
                .Include(a => a.Room)
                .OrderBy(a => a.AssetName)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all assets in a category along with their room details.
        /// </summary>
        public async Task<List<Asset>> GetAllAssetsInCategoryWithRoomAsync(int assetCategoryServerId)
        {
            return await appDbContext.Assets
                .AsNoTracking()
                .Where(a => a.AssetCategoryServerId == assetCategoryServerId)
                .Include(a => a.Room)
                .Include(a => a.AssetCategory)
                .OrderBy(a => a.AssetName)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single asset based on its QR code text.
        /// </summary>
        public async Task<Asset?> GetAssetByQRCodeAsync(string qrCodeSecretText)
        {
            return await appDbContext.Assets
                .AsNoTracking()
                .Include(a => a.Room)
                .Include(a => a.AssetCategory)
                .FirstOrDefaultAsync(a => a.QRCodeSecretText == qrCodeSecretText);
        }
    }
}
