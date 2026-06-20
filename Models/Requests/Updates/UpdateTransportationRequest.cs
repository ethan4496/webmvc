namespace WebMVC.Models.Requests.Updates
{
    public class UpdateTransportationRequest
    {
        public string Username { get; set; }
        public int Status { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public int Quantity { get; set; }
        public decimal Currency { get; set; }
        public decimal? UnitWeight { get; set; }
        public decimal? UnitVolume { get; set; }
        public decimal? Surcharge { get; set; }
        //public decimal? PriceShipping { get; set; }
        //public decimal TotalPriceVND { get; set; }
        public int? FromId { get; set; }
        public int? ToId { get; set; }
        public int? ShipId { get; set; }
        public List<IFormFile> Images { get; set; }

        public string UserNote { get; set; }

    }
}
