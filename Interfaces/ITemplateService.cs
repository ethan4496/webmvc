using WebMVC.Entities;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface ITemplateService
    {
        Task<PagedList<TemplateResponse>> GetPaging(TemplateSearch search);
        Task<List<EmailTemplate>> GetAll();
        Task CreateAsync(CreateTemplateRequest request);
        Task SaveAsync(int id, CreateTemplateRequest request);
        Task<EmailTemplate> GetTemplateById(int id);
        Task DeleteAsync(int id);

    }
}
