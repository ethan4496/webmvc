using WebMVC.Models.Responses;

namespace WebMVC.Models.ViewModels
{
    public class NewsViewModel
    {
        public List<object> Categories { get; set; }
        public PostResponse PrimaryPost { get; set; }
        public List<PostResponse> Posts { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}
