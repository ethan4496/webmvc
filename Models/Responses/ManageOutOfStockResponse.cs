using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class ManageOutOfStockResponse : OutOfStock
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
        public List<TransportationOfOutStockResponse> TransportationResponses { get; set; } = new List<TransportationOfOutStockResponse>();

        public string LineColor
        {
            get
            {
                if (IsPrintTemp == true)
                {
                    return "#feffc6";
                }

                if (IsSend == true)
                {
                    return "aquamarine";
                }
                
                return "unset";

            }
        }
    }

    public class TransportationOfOutStockResponse
    {
        public string Barcode { get; set; }
        public double? Weight { get; set; }
        public double? Volume { get; set; }
        public int? Quantity { get; set; }
        public DateTime? DateCompleted { get; set; }
        public DateTime? DateArrivedAtVNWarehouse { get; set; }
        public string VoucherInfo { get; set; }
        public decimal? UnitWeight { get; set; }
        public decimal? UnitVolume { get; set; }
        public string ShipName { get; set; }

    }
}
