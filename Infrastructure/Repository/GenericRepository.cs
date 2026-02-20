using Application.Helpers;
using Application.Repository;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<bool> SaveAsync()
        {
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<Result<string>> CreateAsync(T entity)
        {
            try
            {
                var record = (await _dbSet.AddAsync(entity)).Entity;

                if (await SaveAsync())
                    return Result<string>.Success($"{typeof(T).Name} Added Successfully");

                return Result<string>.Fail("No changes were saved");
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
        }

        public async Task<Result<string>> UpdateAsync(T entity)
        {
            try
            {
                var keyProperty = _context.Entry(entity).Property("Id").CurrentValue;

                var trackedEntity = _context.ChangeTracker.Entries<T>()
                    .FirstOrDefault(e => e.Property("Id").CurrentValue!.Equals(keyProperty));

                if (trackedEntity != null)
                    trackedEntity.State = EntityState.Detached;

                _dbSet.Update(entity);

                if (await SaveAsync())
                    return Result<string>.Success($"{typeof(T).Name} Updated Successfully");

                return Result<string>.Fail("No changes were saved");
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
        }

        public async Task<Result<string>> DeleteAsync(T entity)
        {
            try
            {
                _dbSet.Remove(entity);

                if (await SaveAsync())
                    return Result<string>.Success($"{typeof(T).Name} Deleted Successfully");

                return Result<string>.Fail("No changes were saved");
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {

            return await _dbSet.AsNoTracking().ToListAsync();
            
        }

        public async Task<T> FindByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<List<T>> GetByAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

    }
}

