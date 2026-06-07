using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    public class WebConfigurationController : Controller
    {
        private readonly IWebConfigurationService _webConfigurationService;

        public WebConfigurationController(IWebConfigurationService webConfigurationService)
        {
            _webConfigurationService = webConfigurationService;
        }

        [Authorize]
        [Route("webconfig")]
        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        public async Task<IActionResult> Index()
        {
            var webConfiguration = await _webConfigurationService.GetById();
            return View(webConfiguration);
        }

        [Authorize]
        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        [HttpPut]
        public async Task<ApiResponse> Update(UpdateWebConfigurationRequest request)
        {
            await _webConfigurationService.Update(request);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }
    }
}
