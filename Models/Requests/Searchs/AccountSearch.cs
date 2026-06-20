namespace WebMVC.Models.Requests.Searchs
{
    public class AccountSearch : PagingSearch
    {
        public int? IsCustomer { get; set; }
        public int? AccountId { get; set; }
        public int? SaleId { get; set; }
        public int? RoleId { get; set; }
        public string PostOffice{ get; set; }
        public string Username { get; set; }
    }
}
