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
        Task<ResponseClass> GetPagingApi(CampaignSearchApi search);
        Task CreateAsync(CreateCampaignRequest request);
        Task<ResponseClass> CreateApiAsync(CreateCampaignApiRequest request);
        Task SaveAsync(int id, CreateCampaignRequest request);
        Task<ResponseClass> SaveApiAsync(int id, CreateCampaignApiRequest request);
        Task<Campaign> GetCampaignById(int id);
        Task<ResponseClass> GetCampaign(int id, AppUser appUser);
        Task DeleteAsync(int id);
        Task<ResponseClass> DeleteApiAsync(int id, AppUser appUser);
        Task<List<object>> GetAllCampaignNames();
        Task<object> GetReportStats(int? campaignId);
        Task<ResponseClass> GetReportStatsApi(ReportStatistic request);
        Task<object> GetReportLogs(int? campaignId, int pageIndex, int pageSize);
        Task<ResponseClass> GetApiLogs(ReportLogs body);
        Task<object> GetEmailTrackingList(int? campaignId, int pageIndex, int pageSize);
        Task<ResponseClass> GetMonthData(AppUser request);
    }
}
