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
        Task<ResponseClass> GetPagingApi(TemplateSearch search);
        Task<List<EmailTemplate>> GetAll();
        Task<ResponseClass> GetAllApi(AppUser appUser);
        Task CreateAsync(CreateTemplateRequest request);
        Task<ResponseClass> CreateApi(CreateTemplateApiRequest request);
        Task SaveAsync(int id, CreateTemplateRequest request);
        Task<ResponseClass> SaveApiAsync(int id, CreateTemplateApiRequest request);
        Task saveSignature(CreateSignatureRequest request);
        Task saveSignatureApi(CreateSignatureApiRequest request);
        Task<EmailTemplate> GetTemplateById(int id);
        Task<TemplateStatusCountResponse> GetStatusCount();
        Task<ResponseClass> GetStatusCountApi(TemplateSearch appUser);
        Task<ResponseClass> GetTemplate(int id, AppUser appUser);
        Task<AccountSignature> GetSignature();
        Task<AccountSignature> GetSignatureApi(AppUser request);
        Task DeleteAsync(int id);
        Task<ResponseClass> DeleteApiAsync(int id, AppUser appUser);

    }
}
