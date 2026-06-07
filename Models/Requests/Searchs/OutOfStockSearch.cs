namespace WebMVC.Models.Requests.Searchs
{
    public class OutOfStockSearch : PagingSearch
    {
        public int? AccountId { get; set; }
        public int? Id { get; set; }
        public string Username { get; set; }
        public string Barcode { get; set; }
        public string PostOffice { get; set; }
        public int? Status { get; set; }
        public int? StatusPayment { get; set; }
    }
}
