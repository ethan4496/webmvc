namespace WebMVC.Models.Requests.Searchs
{
    public class TemplateSearch : PagingSearch
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        public string? Key { get; set;}
        public int? UserId { get; set;}
        public string? SortBy { get; set;}
        
    }
}
