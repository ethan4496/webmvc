namespace WebMVC.Models.Requests.Searchs
{
    public class BigPackageSearch : PagingSearch
    {
        public int? Status { get; set; }
        public string Name { get; set; }
    }
}
