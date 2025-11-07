using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;

namespace KovsieAssetsTracker.Domain.IRepositories
{
    public interface IAssetRepository : IRepositoryBase<Asset>
    {
        /// <summary>
        /// Retrieves all assets with their related room information.
        /// </summary>
        Task<List<Asset>> GetAllAssetsWithRoomAsync();

        /// <summary>
        /// Retrieves all assets with their related category information.
        /// </summary>
        Task<List<Asset>> GetAllAssetsWithCategoryAsync();

        /// <summary>
        /// Retrieves all assets with both category and room relationships.
        /// </summary>
        Task<List<Asset>> GetAllAssetsWithCategoryAndRoomAsync();

        /// <summary>
        /// Retrieves all assets within a specific room.
        /// </summary>
        Task<List<Asset>> GetAllAssetsInRoomAsync(int roomServerId);

        /// <summary>
        /// Retrieves all assets within a specific category.
        /// </summary>
        Task<List<Asset>> GetAllAssetsInCategoryAsync(int assetCategoryServerId);

        /// <summary>
        /// Retrieves all assets within a category, including their room information.
        /// </summary>
        Task<List<Asset>> GetAllAssetsInCategoryWithRoomAsync(int assetCategoryServerId);

        /// <summary>
        /// Retrieves a single asset using its QR code text.
        /// </summary>
        Task<Asset?> GetAssetByQRCodeAsync(string qrCodeSecretText);
    }
}
