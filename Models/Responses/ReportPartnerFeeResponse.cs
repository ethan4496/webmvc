using WebMVC.Entities;

namespace WebMVC.Models.Responses
{
    public class ReportPartnerFeeResponse : ReportPartnerFee
    {
        public string Username { get; set; }
        public string TableTitle { get; set; }

    }
}
