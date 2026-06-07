using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class TrackingResponse
    {
        public int Id { get; set; }
        public string Barcode { get; set; }
        public int Status { get; set; }
        public string WarehouseFrom { get; set; }
        public string WarehouseTo { get; set; }
        public string ShipName { get; set; }
        public int ShipId { get; set; }
        public double? Weight { get; set; }
        public double? Volume { get; set; }
        public int? Quantity { get; set; }
        public string Note { get; set; }

        public DateTime Created { get; set; }
        public DateTime? DateArrivedAtTQWarehouse { get; set; }
        public DateTime? DateExitedFromTQWarehouse { get; set; }
        public DateTime? DateCustomsInspectedGoods { get; set; }
        public DateTime? DateReturningToVNWarehouse { get; set; }
        public DateTime? DateArrivedAtVNWarehouse { get; set; }
        public DateTime? DateCompleted { get; set; }

        public double? UserUploadWeight { get; set; }
        public double? UserUploadVolume { get; set; }
        public int? UserUploadQuantity { get; set; }
        public string UserUploadImage { get; set; }

        public List<TransportationProduct> Products { get; set; } = new List<TransportationProduct>();
    }
}
