using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Tracking : BaseEntity
    {
        public string ProductName { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Weight { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeShipTQ { get; set; }
        public int? Quantity { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeShipVN { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? UnitPriceCYN { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? UnitPriceVND { get; set; }
        public int TransportationId { get; set; }
        public string Note { get; set; }
        public int? VNWarehouseId { get; set; }
        [Column(TypeName = "decimal(18,6)")]
        public decimal? Volume { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeShipVNVolume { get; set; }
    }
}
