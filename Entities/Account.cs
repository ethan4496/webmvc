using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Account : BaseEntity
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordEncryptValue { get; set; }
        public int RoleId { get; set; }
        public string Address { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? SaleId { get; set; }
        public double? MinWeight { get; set; }
        public string PostOffice { get; set; }
        public string DeliveryMethod { get; set; }
        public string Avatar { get; set; }
        public string StaffAvatar { get; set; }
        public string OneSignalId { get; set; }

        // Loại đơn hàng phân theo nhân viên (Lẻ, Lô, ...)
        public int? TransportationType { get; set; }

        public int AppType { get; set; }
        public string AppTypeName { get; set; }
        public string AppToken { get; set; }

        //Danh sách phương thức vận chuyển đặc biệt hiển thị thêm
        public string SpecialShipId { get; set; }
        public int? ToWarehouseId { get; set; }

    }
}
