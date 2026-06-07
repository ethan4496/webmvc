using Microsoft.AspNetCore.SignalR;
using WebMVC.Hubs;
using WebMVC.Interfaces;

namespace WebMVC.Services
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly UserConnectionManager _userConnectionManager;

        public SignalRService(IHubContext<ChatHub> hubContext, UserConnectionManager userConnectionManager)
        {
            _hubContext = hubContext;
            _userConnectionManager = userConnectionManager;
        }
        public async Task SendToastNotificationToUser(string receiverId, string message)
        {
            string receiverConnectionId = _userConnectionManager.GetConnectionId(receiverId);
            if (receiverConnectionId != null)
            {
                await _hubContext.Clients.Client(receiverConnectionId).SendAsync("ReceiveToastNotification", message);
            }
        }
        public async Task SendRemoteLogoutToUser(int receiverId)
        {
            string receiverConnectionId = _userConnectionManager.GetConnectionId(receiverId.ToString());
            if (receiverConnectionId != null)
            {
                await _hubContext.Clients.Client(receiverConnectionId).SendAsync("RemoteLogout");
            }
        }
        public async Task SendToastNotification(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveToastNotification", message);
        }
        public async Task SendConfirmNotificationToUser(string receiveMethod, string receiverId, string message)
        {
            // Kiểm tra xem receiverId có trong danh sách ConnectionId không
            string receiverConnectionId = _userConnectionManager.GetConnectionId(receiverId);
            if (receiverConnectionId != null)
            {
                await _hubContext.Clients.Client(receiverConnectionId).SendAsync(receiveMethod, message);
            }
        }
        public async Task SendConfirmNotification(string receiveMethod, string message)
        {
            await _hubContext.Clients.All.SendAsync(receiveMethod, message);
        }

        public async Task SendScanMobileMessage(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveScanMobile", message);
        }

        public async Task SendScanWebMessage(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveScanWeb", message);
        }
    }
}
