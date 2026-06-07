namespace WebMVC.Models.Requests.Searchs
{
    public class PricingSearch : PagingSearch
    {
        public int? FromId { get; set; }
        public int? ToId { get; set; }
        public int? ShipId { get; set; }
        public int? Type { get; set; }
    }
}
