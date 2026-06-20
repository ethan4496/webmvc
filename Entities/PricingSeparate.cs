using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class PricingSeparate : BaseEntity
    {
        public int AccountId { get; set; }
        public int ShipId { get; set; }
        public double RangeMin { get; set; }
        public double RangeMax { get; set; }
        public int Type { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal PricePerUnit { get; set; }
    }
}
