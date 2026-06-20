namespace WebMVC.Models.Requests.Creates
{
    public class CreateOutOfStockRequest
    {
        public int AccountId { get; set; }
        public List<string>  Barcodes { get; set; } = new List<string>();
    }
}
