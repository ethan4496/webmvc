namespace WebMVC.Models.Requests.Searchs
{
    public class CampaignSearchApi : PagingSearch
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        public string? SortBy { get; set; }

        public AppUser user {get; set;}
        
    }
}
