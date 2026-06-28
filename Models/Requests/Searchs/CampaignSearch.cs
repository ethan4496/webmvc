namespace WebMVC.Models.Requests.Searchs
{
    public class CampaignSearch : PagingSearch
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        
    }
}
