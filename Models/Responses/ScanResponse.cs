using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class ScanResponse
    {
        public int Id { get; set; }
        public string Barcode { get; set; }
        public string BigPackageName { get; set; }
        public string AccountName { get; set; }
        public int? AccountId { get; set; }
        public int? BigPackageId { get; set; }
        public double Quantity { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public decimal Surcharge { get; set; }
        public int Status { get; set; }
        public int? Type { get; set; }
        public string StaffNote { get; set; }
        public string StatusName
        {
            get
            {
                return ETransportationStatusName.GetStatusName(Status);
            }
        }
        public string Image { get; set; }
    }
}
