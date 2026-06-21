namespace WebMVC.Interfaces
{
    public interface IPlainRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(List<T> entities);
        void Remove(T entity);
        void RemoveRange(List<T> entities);
    }
}