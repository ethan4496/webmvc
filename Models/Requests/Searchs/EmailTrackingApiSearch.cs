namespace WebMVC.Models.Requests.Searchs
{
    public class EmailTrackingApiSearch : PagingSearch
    {
        public int UserId { get; set; }
        public string Key { get; set; }
        public int CampaignId { get; set; }
        
    }
}
