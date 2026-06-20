using WebMVC.Entities;

namespace WebMVC.Models.Responses
{
    public class AccountHomeResponse
    {
        public int Id { get; set; }

        public string Username { get; set; }
        public string Avatar { get; set; }
        public string Fullname { get; set; }
        public string RoleName { get; set; }
        public List<CountStatus> CountOrder { get; set; } = new List<CountStatus>();
        public List<Notification> NewNotifications { get; set; } = new List<Notification>();
        public int UnPayOutOfStock { get; set; }
        public int Voucher { get; set; }
    }
    public class CountStatus
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }
}

