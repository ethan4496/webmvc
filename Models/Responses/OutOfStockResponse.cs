using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class OutOfStockResponse : OutOfStock
    {
        public string Username { get; set; }

        public string StatusName
        {
            get
            {
                return EOutOfStockStatusName.GetStatusNameWithColor(Status);
            }
        }
        public string StatusPaymentName
        {
            get
            {
                return EPaymentOutOfStockStatusName.GetStatusNameWithColor(StatusPayment);
            }
        }
        public List<TransportationResponse> TransportationResponses { get; set; } = new List<TransportationResponse>();
    }
}
