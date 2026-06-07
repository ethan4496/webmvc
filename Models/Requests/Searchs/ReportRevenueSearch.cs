namespace WebMVC.Models.Requests.Searchs
{
    public class ReportRevenueSearch : PagingSearch
    {
        public int? PostOffice { get; set; }
        public int? AccountId { get; set; }
        public int? SaleId { get; set; }
        public int? Type { get; set; }
    }
}
