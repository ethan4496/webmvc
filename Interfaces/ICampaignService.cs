using WebMVC.Entities;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface ICampaignService
    {
        Task<PagedList<CampaignResponse>> GetPaging(CampaignSearch search);
        Task CreateAsync(CreateCampaignRequest request);
        Task SaveAsync(int id, CreateCampaignRequest request);
        Task<Campaign> GetCampaignById(int id);
        Task DeleteAsync(int id);

    }
}
