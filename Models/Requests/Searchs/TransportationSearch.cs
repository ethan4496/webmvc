namespace WebMVC.Models.Requests.Searchs
{
    public class TransportationSearch : PagingSearch
    {
        public int? AccountId { get; set; }
        public int? BigPackageId { get; set; }
        public string Barcode { get; set; }
        public string BigPackageName { get; set; }
        public int? Status { get; set; }
        public int? Type { get; set; }
        public string PostOffice { get; set; }
        public bool? IsNotAccount { get; set; }
        public int? ShipId { get; set; }
        public bool? IsOutStock { get; set; }
        public int? FilterDay { get; set; }
    }
}
