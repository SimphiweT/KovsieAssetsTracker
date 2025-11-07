using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories;
using KovsieAssetsTracker.Domain.IRepositories.Base;
using Microsoft.AspNetCore.Identity;

namespace KovsieAssetsTracker.Infrastructure.EF_Repositories.Base
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly AppDbContext appDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        private IAssetRepository? asset;
        private IAssetCategoryRepository? assetCategory;
        private IRoomRepository? room;
        private IFloorRepository? floor;
        private IBuildingRepository? building;
        private ICampusRepository? campus;
        private IUserRepository? user;
        private IUserAccessRequestRepository? userAccessRequest;

        public RepositoryWrapper(AppDbContext _appDbContext, UserManager<ApplicationUser> _userManager)
        {
            appDbContext = _appDbContext;
            userManager = _userManager;
        }

        public IAssetRepository Asset
        {
            get
            {
                if (asset == null)
                {
                    asset = new AssetRepository(appDbContext);
                }
                return asset;
            }
        }
        public IAssetCategoryRepository AssetCategory
        {
            get
            {
                if (assetCategory == null)
                {

                    assetCategory = new AssetCategoryRepository(appDbContext);
                }
                return assetCategory;
            }
        }
        public IRoomRepository Room
        {
            get
            {
                if (room == null)
                {
                    room = new RoomRepository(appDbContext);
                }
                return room;
            }
        }
        public IFloorRepository Floor
        {
            get
            {
                if (floor == null)
                {
                    floor = new FloorRepository(appDbContext);
                }
                return floor;
            }
        }
        public IBuildingRepository Building
        {
            get
            {
                if (building == null)
                {
                    building = new BuildingRepository(appDbContext);
                }
                return building;
            }
        }
        public ICampusRepository Campus
        {
            get
            {
                if (campus == null)
                {
                    campus = new CampusRepository(appDbContext);
                }
                return campus;
            }
        }
        public IUserRepository User
        {
            get
            {
                if (user == null)
                    user = new UserRepository(appDbContext, userManager);
                return user;
            }
        }

        public IUserAccessRequestRepository UserAccessRequest
        {
            get
            {
                if (userAccessRequest == null)
                {
                    userAccessRequest = new UserAccessRequestRepository(appDbContext);
                }
                return userAccessRequest;
            }
        }

        public async Task SaveAsync()
        {
            await appDbContext.SaveChangesAsync();
        }
    }
}
