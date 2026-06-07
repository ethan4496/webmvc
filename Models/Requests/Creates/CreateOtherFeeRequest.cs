namespace WebMVC.Models.Requests.Creates
{
    public class CreateOtherFeeRequest
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int? PostOffice { get; set; }
        public DateTime DataDate { get; set; }
        public IFormFile DetailFile { get; set; }

    }
}
