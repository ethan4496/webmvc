namespace WebMVC.Models.Requests.Creates
{
    public class CreateExpenseRequest
    {
        public string Name { get; set; }
        public decimal Total { get; set; }
        public decimal ExchangeRate { get; set; } = 1;
        public decimal TotalVND { get; set; }
        public int Type { get; set; }
        public int BigPackageId { get; set; }
    }
}
