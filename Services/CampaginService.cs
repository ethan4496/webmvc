using AutoMapper;
using Azure.Core;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;
using static WebMVC.Services.ZaloAPIService;
namespace WebMVC.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;
        private readonly ISignalRService _signalRService;
        private readonly INotificationService _notificationService;
        private readonly IZaloAPIService _zaloAPIService;
        private readonly IUploadFileService _uploadFileService;

        public CampaignService(IUnitOfWork unitOfWork, IMapper mapper,
            IHttpContextService httpContextService, ISignalRService signalRService,
            INotificationService notificationService, IZaloAPIService zaloAPIService,
            IUploadFileService uploadFileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextService = httpContextService;
            _signalRService = signalRService;
            _notificationService = notificationService;
            _zaloAPIService = zaloAPIService;
            _uploadFileService = uploadFileService;
        }

        public async Task<PagedList<CampaignResponse>> GetPaging(CampaignSearch search)
        {
            var loggedModel = _httpContextService.GetLoggedModel();
            return await GetPagingForAPI(search, loggedModel);

        }


        public async Task<PagedList<CampaignResponse>> GetPagingForAPI(CampaignSearch search, LoggedModel loggedModel)
        {
            var query = _unitOfWork.Repository<Campaign>().GetQueryable();
            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                query = query.Where(x =>
                    x.Name.Contains(search.Name));
            }

            if (!string.IsNullOrWhiteSpace(search.Status))
            {
                query = query.Where(x =>
                    x.Status == search.Status);
            }

            if (search.FromDate.HasValue)
            {
                query = query.Where(x =>
                    x.Created >= search.FromDate.Value);
            }

            if (search.ToDate.HasValue)
            {
                var toDate = search.ToDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.Created < toDate);
            }

            query = query.Where(x => x.CreatedBy == loggedModel.Id);

            var totalItems = await query.CountAsync();
            var accountQuery = _unitOfWork.Repository<Account>().GetQueryable();
            var contactListQuery = _unitOfWork.Repository<ContactList>().GetQueryable();
            var items = await query
                .OrderBy(x => x.Id)
                .Skip((search.PageIndex - 1) * search.PageSize)
                .Take(search.PageSize)
                .Join(accountQuery,
                    campaign => campaign.CreatedBy,
                    account => account.Id,
                    (campaign, account) => new { campaign, account })
                .Join(contactListQuery,
                    x => x.campaign.ContactId,
                    cl => cl.Id,
                    (x, cl) => new CampaignResponse
                    {
                        Id = x.campaign.Id,
                        Name = x.campaign.Name,
                        Subject = x.campaign.Subject,
                        Status = x.campaign.Status,
                        Body = x.campaign.Body,
                        Created = x.campaign.Created,
                        CreatedBy = x.campaign.CreatedBy,
                        Updated = x.campaign.Updated,
                        UpdateBy = x.campaign.UpdateBy,
                        SendAt = x.campaign.SendAt,
                        AccountName = x.account.FullName,
                        EmailSent = x.campaign.EmailSent,
                        ContactListName = cl.Name
                    })
                .ToListAsync();
            // Console.WriteLine(
            //     "items"
            // );
            // Console.WriteLine(
            //     JsonConvert.SerializeObject(items, Formatting.Indented)
            // );

            return new PagedList<CampaignResponse>
            {
                Items = items,
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = totalItems
            };
        }
        public async Task<Campaign> GetCampaignById(int id)
        {
            var template = await _unitOfWork.Repository<Campaign>().GetQueryable().FirstOrDefaultAsync(x => x.Id == id);
            return template;
        }
        public async Task CreateAsync(CreateCampaignRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            // Console.WriteLine(
            //     "request"
            // );
            // Console.WriteLine(
            //     JsonConvert.SerializeObject(request, Formatting.Indented)
            // );
            var campagin = new Campaign
            {
                Name = request.Name,
                Subject = request.Subject,
                Body = request.Body,
                Status = request.Status,
                TemplateId = request.TemplateId,
                ContactId = request.ContactId,
                SendAt = request.SendAt,
                EmailSent = request.EmailSent,
                Created = currentDate,
            };
            // Console.WriteLine(
            //     "campagin"
            // );
            // Console.WriteLine(
            //     JsonConvert.SerializeObject(campagin, Formatting.Indented)
            // );

            await _unitOfWork.Repository<Campaign>().Add(campagin, currentDate, currentAccount.Id);

            await _unitOfWork.SaveAsync();
        }
        public async Task SaveAsync(int id, CreateCampaignRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var campagin = new Campaign
            {
                Id = id,
                Name = request.Name,
                Subject = request.Subject,
                Body = request.Body,
                Status = request.Status,
                TemplateId = request.TemplateId,
                ContactId = request.ContactId,
                SendAt = request.SendAt,
                EmailSent = request.EmailSent,
                Updated = currentDate,
            };
            _unitOfWork.Repository<Campaign>().Update(campagin, currentDate, currentAccount.Id);

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Campaign>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new Exception("Không tìm thấy dữ liệu");
            }

            _unitOfWork.Repository<Campaign>().Delete(entity);

            await _unitOfWork.SaveAsync();
        }

        public async Task<List<object>> GetAllCampaignNames()
        {
            return await _unitOfWork.Repository<Campaign>().GetQueryable()
                .OrderByDescending(x => x.Id)
                .Select(x => new { x.Id, x.Name })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<object> GetReportStats(int? campaignId)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var campaignIds = _unitOfWork.Repository<Campaign>().GetQueryable()
                .Where(c => c.CreatedBy == currentAccount.Id)
                .Select(c => c.Id);

            var query = _unitOfWork.Repository<MailLog>().GetQueryable()
                .Where(x => campaignIds.Contains(x.CampaignId));
            if (campaignId.HasValue)
                query = query.Where(x => x.CampaignId == campaignId.Value);

            var total = await query.CountAsync();
            var success = await query.CountAsync(x => x.Status == "success");
            var failed = await query.CountAsync(x => x.Status == "failed");

            return new { total, success, failed };
        }

        public async Task<object> GetReportLogs(int? campaignId, int pageIndex, int pageSize)
        {
            var query = _unitOfWork.Repository<MailLog>().GetQueryable();
            var currentAccount = await _httpContextService.GetCurrentAccount();
            if (campaignId.HasValue)
                query = query.Where(x => x.CampaignId == campaignId.Value);

            var joinedQuery = query
                .Join(_unitOfWork.Repository<Contact>().GetQueryable(),
                    log => log.ContactId, c => c.Id, (log, c) => new { log, c })
                .Join(_unitOfWork.Repository<Campaign>().GetQueryable()
                        .Where(cam => cam.CreatedBy == currentAccount.Id),
                    x => x.log.CampaignId, cam => cam.Id, (x, cam) => new
                    {
                        x.log.Status,
                        x.log.ErrorMessage,
                        x.log.SentAt,
                        ContactName = x.c.FirstName + " " + x.c.LastName,
                        ContactEmail = x.c.Email,
                        EmailSent = cam.EmailSent,
                        CampaignName = cam.Name,
                    });

            var total = await joinedQuery.CountAsync();
            var items = await joinedQuery
                .OrderByDescending(x => x.SentAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new { items, total, pageIndex, pageSize };
        }

    }
}
