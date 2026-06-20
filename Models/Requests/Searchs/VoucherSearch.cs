namespace WebMVC.Models.Requests.Searchs
{
    public class VoucherSearch : PagingSearch
    {
        public int? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
