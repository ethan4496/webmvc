using System.ComponentModel.DataAnnotations;

namespace WebMVC.Models.Requests.Updates
{
    public class UpdateAccountRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string FullName { get; set; }
        public int? RoleId { get; set; }
        public int? SaleId { get; set; }
        public double? MinWeight { get; set; }
        public string PostOffice { get; set; }
        public string DeliveryMethod { get; set; }
        public string NewPassword { get; set; }
        public string SpecialShipId { get; set; }
        public int? TransportationType { get; set; }
        public List<int> VNWarehouseStaffIds { get; set; }
    }
}
