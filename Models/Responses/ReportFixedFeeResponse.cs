using WebMVC.Entities;

namespace WebMVC.Models.Responses
{
    public class ReportFixedFeeResponse : ReportFixedFee
    {
        public string Username { get; set; }
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Chờ duyệt" : "Đã duyệt";
            }
        }
        public string TableTitle { get; set; }
    }
}
