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
    public class ContactService : IContactService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;
        private readonly ISignalRService _signalRService;
        private readonly INotificationService _notificationService;
        private readonly IZaloAPIService _zaloAPIService;
        private readonly IUploadFileService _uploadFileService;

        public ContactService(IUnitOfWork unitOfWork, IMapper mapper,
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

        public async Task<PagedList<ContactListResponse>> GetPaging(CampaignSearch search)
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

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Created)
                .Skip((search.PageIndex - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();
            // Console.WriteLine(
            //     "items"
            // );
            // Console.WriteLine(
            //     JsonConvert.SerializeObject(items, Formatting.Indented)
            // );
            var responseItems = items.Select(x => new CampaignResponse
            {
                Id = x.Id,
                Name = x.Name,
                Subject = x.Subject,
                Status = x.Status,
                Body = x.Body,
                Created = x.Created,
                CreatedBy = x.CreatedBy,
                Updated = x.Updated,
                UpdateBy = x.UpdateBy
            }).ToList();

            return new PagedList<CampaignResponse>
            {
                Items = responseItems,
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
            var template = new Campaign
            {
                Name = request.Name,
                Subject = request.Subject,
                Body = request.Body,
                Status = request.Status,
                Created = currentDate,
            };

            await _unitOfWork.Repository<Campaign>().Add(template, currentDate, currentAccount.Id);

            await _unitOfWork.SaveAsync();
        }
        public async Task SaveAsync(int id, CreateCampaignRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var template = new EmailTemplate
            {
                Id = id, 
                Name = request.Name,
                Subject = request.Subject,
                Body = request.Body,
                Status = request.Status,
                Created = currentDate,
            };
            _unitOfWork.Repository<EmailTemplate>().Update(template, currentDate, currentAccount.Id);

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
        
    }
}
