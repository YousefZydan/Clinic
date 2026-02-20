using Application.Helpers;
using System.Linq.Expressions;

namespace Application.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<Result<string>> CreateAsync(T entity);
        Task<Result<string>> UpdateAsync(T entity);
        Task<Result<string>> DeleteAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> FindByIdAsync(Guid id);
        Task<List<T>> GetByAsync(Expression<Func<T, bool>> predicate);
        Task<bool> SaveAsync();

    }
}

