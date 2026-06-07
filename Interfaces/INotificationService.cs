
using WebMVC.Controllers;
using WebMVC.Entities;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface INotificationService
    {
        Task<PagedList<Notification>> GetPaging(NotificationSearch search);
        Task<int> GetUnRead();
        Task PushMessage(PushMessageRequest request);
        Task PushOneSignalAllUser(string title, string content);
        Task<bool> Read(int id);
        Task<bool> ReadAll();
        Task SendNotification(Notification notification, DateTime currentDate, int currentAccountId, int customerId = 0, int staffId = 0, List<int> staffRoleIds = null);
    }
}
