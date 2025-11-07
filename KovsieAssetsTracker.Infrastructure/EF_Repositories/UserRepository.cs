using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories;
using KovsieAssetsTracker.Infrastructure.Data;
using KovsieAssetsTracker.Infrastructure.EF_Repositories.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KovsieAssetsTracker.Infrastructure.EF_Repositories
{
    public class UserRepository : RepositoryBase<ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UserRepository(AppDbContext appDbContext, UserManager<ApplicationUser> _userManager)
            : base(appDbContext)
        {
            userManager = _userManager;
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user)
        {
            // Note: Password and roles handled in controller/service, not here
            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
            return user;
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await userManager.Users.ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await userManager.FindByIdAsync(id);
        }

        public async Task<ApplicationUser> UpdateUserAsync(ApplicationUser user)
        {
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
            return user;
        }
    }
}
