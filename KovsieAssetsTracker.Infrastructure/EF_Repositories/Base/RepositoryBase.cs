
using KovsieAssetsTracker.Domain.IRepositories.Base;
using KovsieAssetsTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KovsieAssetsTracker.Infrastructure.EF_Repositories.Base
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly AppDbContext appDbContext;
        protected RepositoryBase(AppDbContext _appDbContext)
        {
            appDbContext = _appDbContext;
        }
        public async Task<List<T>> GetAllAsync()
        {
           return await appDbContext.Set<T>().ToListAsync();
        }
        public async Task<T> GetByIdAsync(int id)
        {
            return await appDbContext.Set<T>().FindAsync(id);
        }
        public async Task<List<T>> GetAllByCondition(Expression<Func<T, bool>> expression)
        {
            return await appDbContext.Set<T>().Where(expression).ToListAsync();
        }
        public async Task<T> CreateAsync(T entity)
        {
            await appDbContext.Set<T>().AddAsync(entity); 
            
            return entity;
        }
        public Task<T> UpdateAsync(T entity)
        {
           appDbContext.Set<T>().Update(entity);

            return Task.FromResult(entity);
        }
        public Task<T> DeleteAsync(T entity)
        {
            appDbContext.Set<T>().Remove(entity);

            return Task.FromResult(entity);
        }
    
    }
}
