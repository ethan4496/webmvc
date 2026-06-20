using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class VoucherResponse : Voucher
    {
        public string StatusName
        {
            get
            {
                return EVoucherStatusName.GetStatusName(Status);
            }
        }

        public int Quantity { get; set; }
    }
}
