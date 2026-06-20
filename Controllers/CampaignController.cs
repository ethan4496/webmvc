using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [Authorize]
    
    public class CampaignController : Controller
    {
        private readonly ICampaignService _campaignService;
        private readonly ITemplateService _templateService;
        public CampaignController(ICampaignService campaginService, ITemplateService templateService)
        {
            _campaignService = campaginService;
            _templateService = templateService;
        }

        [Route("campagins")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var templates = await _templateService.GetAll();
            return View(templates);
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplatePaging(CampaignSearch search)
        {
            var data = await _campaignService.GetPaging(search);
            return Json(new
            {
                items = data.Items,
                currentPage = search.PageIndex,
                pageSize = search.PageSize,
                totalPages = data.TotalPage,
                totalItems = data.TotalItem
            });
        }

        
        [Route("create-campagin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        
        [Route("campagins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCampaignRequest request)
        {
            try
            {
                await _campaignService.CreateAsync(request);

                TempData["SuccessMessage"] = "Tạo template thành công";

                return Redirect("/templates");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(request);
            }
        }
        [Route("edit-campaign/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();
            var template = await _campaignService.GetCampaignById(id);
            if (template == null)
                return NotFound();
            return View(template);
        }

        [Route("edit-campaign/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, CreateCampaignRequest request)
        {
            try
            {
                await _campaignService.SaveAsync(id, request);
                TempData["SuccessMessage"] = "Save template thành công";
                return Redirect("/templates");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var template = await _campaignService.GetCampaignById(id);
                return View("Edit", template);
            }
        }
        [HttpDelete]
        public async Task<ApiResponse> Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _campaignService.DeleteAsync(id);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Xóa thành công",
                Type = (int)EApiResponseType.Success,
            };
        }
    }
}
