using Microsoft.EntityFrameworkCore;
using WebMVC.Data;
using WebMVC.Entities.Base;
using WebMVC.Interfaces;

namespace WebMVC.Services
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _dbContext;

        public GenericRepository(AppDbContext context)
        {
            _dbContext = context;
        }

        public IQueryable<T> GetQueryable()
        {
            return _dbContext.Set<T>();
        }

        public async Task Add(T entity, DateTime currentDate, int currentAccountId)
        {
            entity.Created = currentDate;
            entity.CreatedBy = currentAccountId;
            await _dbContext.Set<T>().AddAsync(entity);
        }
        public async Task AddRange(List<T> entities, DateTime currentDate, int currentAccountId)
        {
            entities.ForEach(entity =>
            {
                entity.Created = currentDate;
                entity.CreatedBy = currentAccountId;
            });
            await _dbContext.Set<T>().AddRangeAsync(entities);
        }

        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }
        public void DeleteRange(List<T> entities)
        {
            _dbContext.Set<T>().RemoveRange(entities);
        }
        public void Update(T entity, DateTime currentDate, int currentAccountId)
        {
            entity.Updated = currentDate;
            entity.UpdateBy = currentAccountId;
            _dbContext.Set<T>().Update(entity);
        }
        public void UpdateRange(List<T> entities, DateTime currentDate, int currentAccountId)
        {
            entities.ForEach(entity =>
            {
                entity.Updated = currentDate;
                entity.UpdateBy = currentAccountId;
            });
            _dbContext.Set<T>().UpdateRange(entities);
        }

    }
}
