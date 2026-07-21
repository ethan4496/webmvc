using WebMVC.Models.Responses;

namespace WebMVC.Models.ViewModels
{
    public class NewsViewModel
    {
        public List<object> Categories { get; set; }
        public PostResponse PrimaryPost { get; set; }
        public List<PostResponse> Posts { get; set; }
    }
}
