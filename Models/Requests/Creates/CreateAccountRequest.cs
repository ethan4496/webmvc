using System.ComponentModel.DataAnnotations;

namespace WebMVC.Models.Requests.Creates
{
    public class CreateAccountRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string FullName { get; set; }
        public int? SaleId { get; set; }
        public string PostOffice { get; set; }
        public string DeliveryMethod { get; set; }
    }
}
