namespace WebMVC.Models.Requests.Updates
{
    public class AssignTransportationRequest
    {
        public string Barcode { get; set; }
        public string AccountName { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public int ShipId { get; set; }
    }
}
