using WebMVC.Data;
using WebMVC.Interfaces;

namespace WebMVC.Services
{
    public class PlainRepository<T> : IPlainRepository<T> where T : class
    {
        private readonly AppDbContext _dbContext;

        public PlainRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<T> GetQueryable()
        {
            return _dbContext.Set<T>().AsQueryable();
        }

        public async Task AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
        }

        public async Task AddRangeAsync(List<T> entities)
        {
            await _dbContext.Set<T>().AddRangeAsync(entities);
        }

        public void Remove(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public void RemoveRange(List<T> entities)
        {
            _dbContext.Set<T>().RemoveRange(entities);
        }
    }
}