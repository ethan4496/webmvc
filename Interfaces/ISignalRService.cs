
namespace WebMVC.Interfaces
{
    public interface ISignalRService
    {
        Task SendConfirmNotification(string receiveMethod, string message);
        Task SendConfirmNotificationToUser(string receiveMethod, string receiverId, string message);
        Task SendRemoteLogoutToUser(int receiverId);
        Task SendScanMobileMessage(string message);
        Task SendScanWebMessage(string message);
        Task SendToastNotification(string message);
        Task SendToastNotificationToUser(string receiverId, string message);
    }
}
