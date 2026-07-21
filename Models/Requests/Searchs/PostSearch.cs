namespace WebMVC.Models.Requests.Searchs
{
    public class PostSearch : PagingSearch
    {
        public string? Title { get; set; }
        public int? CategoryId { get; set; }

    }
}
