using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class TransportationResponse : Transportation
    {
        public string StatusName
        {
            get
            {
                if(IsOutStock == true && Status == (int)ETransportationStatus.ArrivedAtVNWarehouse)
                {
                    return "<span class=\"badge bg-purple text-white p-2\">Đã yêu cầu</span>";
                }
                return ETransportationStatusName.GetStatusNameWithColor(Status);
            }
        }
        public string StatusNameNotColor
        {
            get
            {
                if (IsOutStock == true && Status == (int)ETransportationStatus.ArrivedAtVNWarehouse)
                {
                    return "Đã yêu cầu";
                }
                return ETransportationStatusName.GetStatusName(Status);
            }
        }
        public decimal SurchargeVND
        {
            get
            {
                return Math.Round((Surcharge ?? 0) * Currency, 0);
            }
        }
        public string AccountName { get; set; }
        public string WarehouseFrom { get; set; }
        public string WarehouseTo { get; set; }
        public string ShipName { get; set; }
        public string BigPackageName { get; set; }
        public string PartnerInfor { get; set; }    

        public List<TransportationHistory> TransportationHistories { get; set; }

        public string DateArrivedAtVNWarehouseString
        {
            get
            {
                return DateArrivedAtVNWarehouse?.ToString("dd/MM/yyyy HH:mm");
            }
        }
        public List<TransportationProduct> Products { get; set; }

        public int? OutOfStockId { get; set; }
    }
}
