using WebMVC.Entities;
using WebMVC.Models;

namespace WebMVC.Interfaces
{
    public interface IHttpContextService
    {
        Task<Account> GetCurrentAccount();
        LoggedModel GetLoggedModel();
    }
}
