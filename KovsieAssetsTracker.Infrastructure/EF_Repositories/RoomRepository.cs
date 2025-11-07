using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories;
using KovsieAssetsTracker.Infrastructure.EF_Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace KovsieAssetsTracker.Infrastructure.EF_Repositories
{
    public class RoomRepository : RepositoryBase<Room>, IRoomRepository
    {
        public RoomRepository(AppDbContext context) : base(context) { }

        public async Task<List<Room>> GetAllRoomsWithAssetsAsync()
        {
            return await appDbContext.Rooms
                .AsNoTracking()
                .Include(r => r.Assets)
                .OrderBy(r => r.RoomName)
                .ToListAsync();
        }

        public async Task<List<Room>> GetAllRoomsWithFloorAndAssetsAsync()
        {
            return await appDbContext.Rooms
                .AsNoTracking()
                .Include(r => r.Floor)
                .Include(r => r.Assets)
                .OrderBy(r => r.RoomName)
                .ToListAsync();
        }

        public async Task<Room?> GetRoomWithAssetsAsync(int roomServerId)
        {
            return await appDbContext.Rooms
                .AsNoTracking()
                .Include(r => r.Assets)
                .Include(r => r.Floor)
                .FirstOrDefaultAsync(r => r.RoomServerId == roomServerId);
        }

        public async Task<List<Room>> GetRoomsWithFloorOnlyAsync()
        {
            return await appDbContext.Rooms
                .AsNoTracking()
                .Include(r => r.Floor)
                .OrderBy(r => r.RoomName)
                .ToListAsync();
        }
    }
}
