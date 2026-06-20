using WebMVC.Entities;

namespace WebMVC.Models.Responses
{
    public class AccountProfile
    {
        public string Username { get; set; }
        public string Address { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public IFormFile FileNewAvatar { get; set; }
        public int? ToWarehouseId { get; set; }
        public List<Warehouse> Warehouses { get; set; }
    }
}
