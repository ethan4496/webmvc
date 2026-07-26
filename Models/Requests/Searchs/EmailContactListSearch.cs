namespace WebMVC.Models.Requests.Searchs
{
    public class EmailContactListSearch
    {
        public string? Email {get; set;}
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; }

        public AppUser user {get; set;}
        
    }
}
