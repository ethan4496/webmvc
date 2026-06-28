namespace WebMVC.Interfaces
{
    public interface IPlainRepository<T> where T : class
    {
        IQueryable<T> GetQueryable();
        Task AddAsync(T entity);
        Task AddRangeAsync(List<T> entities);
        void Remove(T entity);
        void RemoveRange(List<T> entities);
    }
}