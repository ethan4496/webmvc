using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class VoucherAccountResponse : VoucherAccount
    {
        public string StatusName
        {
            get
            {
                return EVoucherAccountStatusName.GetStatusName(Status);
            }
        }
        public string Username { get; set; }
    }
}
