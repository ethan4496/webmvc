namespace WebMVC.Models.Requests.Creates
{
    public class CreateTrackingRequest
    {
        public string Barcode { get; set; }
        public int? Warehouse { get; set; }
        public string ProductName { get; set; }
        public double? TotalWeight { get; set; }
        public double? TotalVolume { get; set; }
        public int? Number { get; set; }
        public decimal? TotalShip { get; set; }
        public decimal? ShipInVn { get; set; }
        public string Note { get; set; }
    }

}
