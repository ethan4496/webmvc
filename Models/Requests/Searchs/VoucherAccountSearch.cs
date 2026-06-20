namespace WebMVC.Models.Requests.Searchs
{
    public class VoucherAccountSearch : PagingSearch
    {
        public int Id { get; set; }
        public int? AccountId { get; set; }
        public int? Status { get; set; }
    }
}
