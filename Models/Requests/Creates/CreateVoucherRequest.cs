namespace WebMVC.Models.Requests.Creates
{
    public class CreateVoucherRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public IFormFile Image { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public IFormFile File { get; set; }
    }
}
