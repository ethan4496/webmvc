using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Pricing : BaseEntity
    {
        public double RangeMin { get; set; }
        public double RangeMax { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal PricePerUnit { get; set; }
        public int Type { get; set; }

        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int ShipId { get; set; }
    }
}
