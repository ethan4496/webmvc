using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedList<CategoryResponse>> GetPaging(CategorySearch search);
        Task<Category> GetCategoryById(int id);
        Task<List<object>> GetAllCategoryNames();
        Task CreateAsync(CreateCategoryRequest request);
        Task SaveAsync(int id, UpdateCategoryRequest request);
        Task DeleteAsync(int id);
    }
}
