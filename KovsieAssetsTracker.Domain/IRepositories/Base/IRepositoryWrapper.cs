using KovsieAssetsTracker.Domain.IRepositories;

namespace KovsieAssetsTracker.Domain.IRepositories.Base
{
    public interface IRepositoryWrapper
    {
        IAssetRepository Asset { get; }
        IAssetCategoryRepository AssetCategory { get; }
        IRoomRepository Room { get; }
        IFloorRepository Floor { get; }
        IBuildingRepository Building { get; }
        ICampusRepository Campus { get; }
        IUserRepository User { get; }
        IUserAccessRequestRepository UserAccessRequest { get; }
        Task SaveAsync();
    }
}
