using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class OutOfStock : BaseEntity
    {
        public int AccountId { get; set; }
        public int Status { get; set; }
        public int StatusPayment { get; set; }
        public DateTime? DateOutStock { get; set; }
        public DateTime? DatePayment { get; set; }
        public string AccountPayment { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalPriceVND { get; set; } = 0;
        public string PostOffice { get; set; }
        public string DeliveryMethod { get; set; }
        public string DeliveryInfo { get; set; }
        public bool? IsRequest { get; set; } = false;
        public bool? IsSend { get; set; } = false;
        public bool? IsPrintTemp { get; set; } = false;
        public DateTime? DateCallPhone { get; set; }
    }
}
