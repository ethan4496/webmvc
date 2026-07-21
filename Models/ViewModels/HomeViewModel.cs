using WebMVC.Models.Responses;

namespace WebMVC.Models.ViewModels
{
    public class HomeViewModel
    {
        public string AppNotiImage { get; set; }
        public List<PostResponse> LatestPosts { get; set; } = new();
    }
}
