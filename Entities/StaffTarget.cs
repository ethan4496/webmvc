using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class StaffTarget : BaseEntity
    {
        public int? AccountId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? Order { get; set; }
        public int? NewAccount { get; set; }
        public double? Weight { get; set; }
        public double? Volume { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPriceVND { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPriceVNDCN { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPriceVNDHT { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPriceVNDDT { get; set; }
    }
}
