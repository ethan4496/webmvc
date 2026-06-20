using WebMVC.Models.Requests.Creates;

namespace WebMVC.Models.Requests.Updates
{
    public class UpdateVoucherRequest : CreateVoucherRequest
    {
        public int Status { get; set; }
    }
}
