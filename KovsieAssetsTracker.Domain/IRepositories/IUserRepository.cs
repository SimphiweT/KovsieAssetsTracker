
using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;

namespace KovsieAssetsTracker.Domain.IRepositories
{
    public interface IUserRepository : IRepositoryBase<ApplicationUser>
    {
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> UpdateUserAsync(ApplicationUser user);
        Task<ApplicationUser> CreateUserAsync(ApplicationUser user);
    }
}
