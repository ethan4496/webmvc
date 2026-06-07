using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Voucher : BaseEntity
    {
        public string Name { get; set; }
        public int Status { get; set; }
        public string Image { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
    }
}
