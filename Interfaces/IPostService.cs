using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IPostService
    {
        Task<PagedList<PostResponse>> GetPaging(PostSearch search);
        Task CreateAsync(CreatePostRequest request);
        Task<PostResponse> GetPostById(int id);
        Task<PostResponse> GetPostBySlug(string slug);
        Task SaveAsync(int id, UpdatePostRequest request);
        Task DeleteAsync(int id);

    }
}
