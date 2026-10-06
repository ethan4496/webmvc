namespace WebMVC.Models.Requests.Updates
{
    public class UpdateExpenseRequest
    {
        public string Name { get; set; }
        public decimal Total { get; set; }
        public decimal ExchangeRate { get; set; } = 1;
        public decimal TotalVND { get; set; }
        public int Type { get; set; }
    }
}
