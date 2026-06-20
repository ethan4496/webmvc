using System.ComponentModel.DataAnnotations.Schema;

namespace WebMVC.Models.Requests.Creates
{
    public class CreatePartnerFeeRequest
    {
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public string Note { get; set; }
        public int? PostOffice { get; set; }
        public DateTime DataDate { get; set; }
    }
}
