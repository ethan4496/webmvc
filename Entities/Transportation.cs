using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Transportation : BaseEntity
    {
        public string Barcode { get; set; }
        // public string? hscode { get; set; }
        public string UserNote { get; set; }
        public string StaffNote { get; set; }
        public int Status { get; set; }
        public int? Type { get; set; }

        public double? Weight { get; set; } = 0;
        public double? Volume { get; set; } = 0;
        public int? Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Currency { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? UnitWeight { get; set; } = 0;
        [Column(TypeName = "decimal(18,0)")]
        public decimal? UnitVolume { get; set; } = 0;
        [Column(TypeName = "decimal(18,2")]
        public decimal? Surcharge { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? PriceShipping { get; set; } = 0;
        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalPriceVND { get; set; } = 0;
        [Column(TypeName = "decimal(18,0)")]
        public decimal Discount { get; set; } = 0;
        public string VoucherInfo { get; set; }
        public string PostOffice { get; set; }
        public string Image { get; set; }

        public int? AccountId { get; set; }
        public int? StaffId { get; set; }
        public int? FromId { get; set; }
        public int? ToId { get; set; }
        public int? ShipId { get; set; }
        public int? VoucherId { get; set; }
        public int? BigPackageId { get; set; }

        public DateTime? DateCancel { get; set; }
        public DateTime? DateArrivedAtTQWarehouse { get; set; }
        public DateTime? DateExitedFromTQWarehouse { get; set; }
        public DateTime? DateCustomsInspectedGoods { get; set; }
        public DateTime? DateReturningToVNWarehouse { get; set; }
        public DateTime? DateArrivedAtVNWarehouse { get; set; }
        public DateTime? DateCompleted { get; set; }

        public string AccountArrivedAtTQWarehouse { get; set; }
        public string AccountExitedFromTQWarehouse { get; set; }
        public string AccountCustomsInspectedGoods { get; set; }
        public string AccountReturningToVNWarehouse { get; set; }
        public string AccountArrivedAtVNWarehouse { get; set; }
        public string AccountCompleted { get; set; }

        public double? UserUploadWeight { get; set; }
        public double? UserUploadVolume { get; set; }
        public int? UserUploadQuantity { get; set; }
        public string UserUploadImage { get; set; }

        public bool? IsOutStock { get; set; }
    }
}
