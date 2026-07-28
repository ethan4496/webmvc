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
            return await GetPagingForAPI(search, loggedModel.Id);

        }
        public async Task<ResponseClass> GetPagingApi(CampaignSearchApi search)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(search.user.Key, (int) search.user.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var request = new CampaignSearch
            {
                Name = search.Name,
                Status = search.Status,
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
            };
            rs.data = await GetPagingForAPI(request, currentAccount.Id);
            return rs;
        }
        

        public async Task<PagedList<CampaignResponse>> GetPagingForAPI(CampaignSearch search, int accountId)
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

            query = query.Where(x => x.CreatedBy == accountId);

            var accountQuery = _unitOfWork.Repository<Account>().GetQueryable();
            var contactListQuery = _unitOfWork.Repository<ContactList>().GetQueryable();
            var emailTrackingQuery = _unitOfWork.PlainRepository<EmailTracking>().GetQueryable();

            var joinedQuery = query
                .Join(accountQuery,
                    campaign => campaign.CreatedBy,
                    account => account.Id,
                    (campaign, account) => new { campaign, account })
                .Join(contactListQuery,
                    x => x.campaign.ContactId,
                    cl => cl.Id,
                    (x, cl) => new { x.campaign, x.account, cl });

            var totalItems = await joinedQuery.CountAsync();

            joinedQuery = search.SortBy switch
            {
                "name" => joinedQuery.OrderBy(x => x.campaign.Name),
                "date" => joinedQuery.OrderByDescending(x => x.campaign.Created),
                _ => joinedQuery.OrderByDescending(x => x.campaign.Id)
            };

            var items = await joinedQuery
                .Skip((search.PageIndex - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(x => new CampaignResponse
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
                        ContactId = x.campaign.ContactId,
                        TemplateId = x.campaign.TemplateId,
                        ContactListName = x.cl.Name,
                        ContactEmailCount = x.cl.ContactListContacts.Count(),
                        EmailTrackingCount = emailTrackingQuery.Count(et => et.CampaignId == x.campaign.Id)
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
        public async Task<ResponseClass> GetCampaign(int id, AppUser appUser)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(appUser.Key, appUser.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var campaign = await _unitOfWork.Repository<Campaign>().GetQueryable().FirstOrDefaultAsync(x => x.Id == id);
            rs.data = campaign;
            return rs;
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

        public async Task<ResponseClass> CreateApiAsync(CreateCampaignApiRequest body)
        {
            var currentDate = DateTime.Now;
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var request = body.request;
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

            await _unitOfWork.Repository<Campaign>().Add(campagin, currentDate, currentAccount.Id);

            await _unitOfWork.SaveAsync();
            rs.data = campagin;
            return rs;
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
                CreatedBy = currentAccount.Id
            };
            _unitOfWork.Repository<Campaign>().Update(campagin, currentDate, currentAccount.Id);

            await _unitOfWork.SaveAsync();
        }

        public async Task<ResponseClass> SaveApiAsync(int id, CreateCampaignApiRequest body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var currentDate = DateTime.Now;
            var request = body.request;
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
                CreatedBy = currentAccount.Id
            };
            _unitOfWork.Repository<Campaign>().Update(campagin, currentDate, currentAccount.Id);

            await _unitOfWork.SaveAsync();
            rs.data = campagin;
            return rs;
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

        public async Task<ResponseClass> DeleteApiAsync(int id, AppUser appUser)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(appUser.Key, appUser.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var entity = await _unitOfWork.Repository<Campaign>().GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new Exception("Không tìm thấy dữ liệu");
            }

            _unitOfWork.Repository<Campaign>().Delete(entity);

            await _unitOfWork.SaveAsync();
            return rs;
        }
        

        public async Task<List<object>> GetAllCampaignNames()
        {
            return await _unitOfWork.Repository<Campaign>().GetQueryable()
                .OrderByDescending(x => x.Id)
                .Select(x => new { x.Id, x.Name })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<ResponseClass> GetReportStatsApi(ReportStatistic body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            int campaignId = (int) body?.CampaignId;
            var campaignIds = _unitOfWork.Repository<Campaign>().GetQueryable()
                .Where(c => c.CreatedBy == currentAccount.Id)
                .Select(c => c.Id);

            var query = _unitOfWork.Repository<MailLog>().GetQueryable()
                .Where(x => campaignIds.Contains(x.CampaignId));
            if (campaignId > 0)
                query = query.Where(x => x.CampaignId == campaignId);

            var total = await query.CountAsync();
            var success = await query.CountAsync(x => x.Status == "success");
            var failed = await query.CountAsync(x => x.Status == "failed");

            var trackingQuery = _unitOfWork.PlainRepository<EmailTracking>().GetQueryable()
                .Where(x => campaignIds.Contains(x.CampaignId));
            if (campaignId > 0)
                trackingQuery = trackingQuery.Where(x => x.CampaignId == campaignId);

            var trackingCount = await trackingQuery.CountAsync();

            rs.data = new { total, success, failed, trackingCount };
            return rs;
        }

        public async Task<ResponseClass> GetMonthData(AppUser body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }

            var campaignIds = _unitOfWork.Repository<Campaign>().GetQueryable()
                .Where(c => c.CreatedBy == currentAccount.Id)
                .Select(c => c.Id);

            var currentYear = DateTime.Now.Year;
            var query = _unitOfWork.Repository<MailLog>().GetQueryable()
                .Where(x => campaignIds.Contains(x.CampaignId) && x.SentAt.Year == currentYear);

            var grouped = await query
                .GroupBy(x => new { x.SentAt.Year, x.SentAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Success = g.Count(x => x.Status == "success"),
                    Failed = g.Count(x => x.Status == "failed")
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            rs.data = grouped.Select(x => new
            {
                Label = $"{x.Month:D2}/{x.Year}",
                x.Success,
                x.Failed
            });
            return rs;
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

        public async Task<ResponseClass> GetApiLogs(ReportLogs body)
        {
            var rs = new ResponseClass();
            var currentAccount = await _httpContextService.GetSessionAsync(body.Key, body.UserId);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var query = _unitOfWork.Repository<MailLog>().GetQueryable();
            if (body?.campaignId.HasValue == true)
                query = query.Where(x => x.CampaignId == body.campaignId.Value);

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
                .Skip((body.pageIndex - 1) * body.pageSize)
                .Take(body.pageSize)
                .ToListAsync();
            rs.data = new { items, total, body.pageIndex, body.pageSize };
            return rs;
        }

        public async Task<object> GetEmailTrackingList(int? campaignId, int pageIndex, int pageSize)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();

            var query = _unitOfWork.PlainRepository<EmailTracking>().GetQueryable()
                .Join(_unitOfWork.Repository<Campaign>().GetQueryable()
                        .Where(c => c.CreatedBy == currentAccount.Id),
                    et => et.CampaignId, c => c.Id, (et, c) => new { et, c });

            if (campaignId.HasValue)
                query = query.Where(x => x.et.CampaignId == campaignId.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.et.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.et.Id,
                    x.et.CampaignId,
                    CampaignName = x.c.Name,
                    x.et.RecipientEmail,
                    x.et.OpenCount,
                    x.et.CreatedAt
                })
                .ToListAsync();

            return new { items, total, pageIndex, pageSize };
        }

    }
}
