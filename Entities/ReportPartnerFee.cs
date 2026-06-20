using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class ReportPartnerFee : BaseEntity
    {
        public string Code { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal Amount { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public string Note { get; set; }
        public int? PostOffice { get; set; }
        public DateTime DataDate { get; set; }
    }
}
