namespace WebMVC.Models.Responses
{
    public class ReportRevenueResponse
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public int Order { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public decimal TotalPriceVND { get; set; }
        public string TableTitle{ get; set; }
    }
}
