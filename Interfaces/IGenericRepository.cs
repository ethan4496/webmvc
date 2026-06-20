using System.Collections.Generic;

namespace WebMVC.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetQueryable();
        Task Add(T entity, DateTime currentDate, int currentAccountId);
        Task AddRange(List<T> entities, DateTime currentDate, int currentAccountId);
        void Update(T entity, DateTime currentDate, int currentAccountId);
        void UpdateRange(List<T> entities, DateTime currentDate, int currentAccountId);
        void Delete(T entity);
        void DeleteRange(List<T> entities);
    }
}
