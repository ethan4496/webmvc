namespace WebMVC.Models.Requests.Updates
{
    public class UpdateTransportationAtOutOfStockManageRequest
    {
        public string Barcode { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public decimal UnitWeight { get; set; }
        public decimal UnitVolume { get; set; }
    }
}
