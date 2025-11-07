using System.Linq.Expressions;

namespace KovsieAssetsTracker.Domain.IRepositories.Base
{
    public interface IRepositoryBase<T>
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAllByCondition(Expression<Func<T, bool>> expression);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(T entity);
    }
}
