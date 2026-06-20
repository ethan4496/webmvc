using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Responses;
using WebMVC.Services;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        int pageSize = 20;
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [Route("notification")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ApiResponse> GetUnRead()
        {
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
                Data = await _notificationService.GetUnRead()
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificationPaging(NotificationSearch search)
        {
            search.PageSize = pageSize;
            var data = await _notificationService.GetPaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_NotificationTable", data.Items);
        }

        [HttpGet]
        public async Task<ApiResponse> Paging(NotificationSearch search)
        {
            search.PageSize = pageSize;
            var data = await _notificationService.GetPaging(search);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
                Data = data.Items
            };
        }

        [HttpPut]
        public async Task<ApiResponse> Read(int id)
        {
            await _notificationService.Read(id);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> ReadAll()
        {
            await _notificationService.ReadAll();
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ApiResponse> PushMessage([FromBody] PushMessageRequest request)
        {
            await _notificationService.PushMessage(request);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }
    }

    public class PushMessageRequest
    {
        public List<int> AccountId { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
    }
}
