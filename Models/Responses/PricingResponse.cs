using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class PricingResponse : Pricing
    {
        public string TypeName
        {
            get
            {
                return EPricingTypeName.GetPricingTypeName(Type);
            }
        }
        public string FromWarehouseName { get; set; }
        public string ToWarehouseName { get; set; }
        public string ShipName { get; set; }
        public int? AccountId { get; set; }
    }
}
