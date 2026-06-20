using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Interfaces;
using WebMVC.Ultilities;

namespace WebMVC.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IHttpContextService _httpContextService;
        private readonly UserConnectionManager _userConnectionManager;

        public ChatHub(IHttpContextService httpContextService,UserConnectionManager userConnectionManager)
        {
            _httpContextService = httpContextService;
            _userConnectionManager = userConnectionManager;
        }

        // Lưu thông tin người dùng khi kết nối
        public override Task OnConnectedAsync()
        {
            string userId = _httpContextService.GetLoggedModel().Id.ToString();

            // Lưu ConnectionId của người dùng
            _userConnectionManager.AddConnection(userId, Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        // Xóa người dùng khi ngắt kết nối
        public override Task OnDisconnectedAsync(Exception exception)
        {
            string userId = _httpContextService.GetLoggedModel().Id.ToString();

            // Xóa người dùng khỏi danh sách ConnectionId khi ngắt kết nối
            _userConnectionManager.RemoveConnection(userId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendToClient(string senderId, string receiverId)
        {
            string receiverConnectionId = _userConnectionManager.GetConnectionId(receiverId);
            if (receiverConnectionId != null)
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessageNotification", senderId);
            }
        }
    }
}
