namespace WebMVC.Models.Requests.Searchs
{
    public class ReportFeeSearch : PagingSearch
    {
        public int? Type { get; set; }
        public int? DataType { get; set; }
    }
}
