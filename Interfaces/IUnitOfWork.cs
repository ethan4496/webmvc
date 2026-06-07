using Microsoft.EntityFrameworkCore.Infrastructure;
using WebMVC.Data;
using WebMVC.Entities.Base;

namespace WebMVC.Interfaces
{
    public interface IUnitOfWork 
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> SaveAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        AppDbContext GetDbContext();
        Task<int> ExecuteSqlRawAsync(string sql);
    }

}
