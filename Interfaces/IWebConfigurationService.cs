using WebMVC.Entities;
using WebMVC.Models.Requests.Updates;

namespace WebMVC.Interfaces
{
    public interface IWebConfigurationService
    {
        Task<WebConfiguration> GetById(int id = 1);
        Task<bool> Update(UpdateWebConfigurationRequest request);
    }
}
