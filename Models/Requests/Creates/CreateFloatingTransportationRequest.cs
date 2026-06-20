namespace WebMVC.Models.Requests.Creates
{
    public class CreateFloatingTransportationRequest
    {
        public string Barcode { get; set; }
        public int Type { get; set; }
        public int? BigPackageId { get; set; }
    }
}
