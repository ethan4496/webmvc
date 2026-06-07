namespace WebMVC.Models.Requests.Updates
{
    public class UpdateBarcodeRequest
    {
        public string Barcode { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public decimal Surcharge { get; set; }
        public int? Status { get; set; }
        public int? Type { get; set; }
        public string Image { get; set; }
        public string StaffNote { get; set; }

        public int? Id { get; set; }
        public int? ShipId { get; set; }

        public string AccountName { get; set; }
    }
}
