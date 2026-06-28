namespace WebMVC.Models.Requests.Searchs
{
    public class TemplateSearch : PagingSearch
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        
    }
}
