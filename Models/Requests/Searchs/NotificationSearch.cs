namespace WebMVC.Models.Requests.Searchs
{
    public class NotificationSearch : PagingSearch
    {
        public int? Type { get; set; }
        public bool? IsRead { get; set; }
    }
}
