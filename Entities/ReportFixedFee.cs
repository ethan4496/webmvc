using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class ReportFixedFee : BaseEntity
    {
        public string Name { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal Amount { get; set; }
        public int Status { get; set; } = 1;
        public string DetailFile { get; set; }
        public int? PostOffice { get; set; }
        public DateTime DataDate { get; set; }
    }
}
