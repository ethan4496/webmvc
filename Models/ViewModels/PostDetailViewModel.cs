using WebMVC.Models.Responses;

namespace WebMVC.Models.ViewModels
{
    public class PostDetailViewModel
    {
        public PostResponse Post { get; set; }
        public List<object> Categories { get; set; }
    }
}
