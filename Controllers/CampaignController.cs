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
using WebMVC.Models.ViewModels;
using System.Text.Json;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [Authorize]

    public class CampaignController : Controller
    {
        private readonly ICampaignService _campaignService;
        private readonly ITemplateService _templateService;
        private readonly IContactService _contactService;

        public CampaignController(ICampaignService campaginService, ITemplateService templateService, IContactService contactService)
        {
            _campaignService = campaginService;
            _templateService = templateService;
            _contactService = contactService;
        }

        [Route("campagins")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            return View();
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
            var templates = await _templateService.GetAll();
            var contacts = await _contactService.GetAll();
            var viewModel = new CreateCampaignViewModel
            {
                Templates = templates,
                ContactLists = contacts
            };
            return View(viewModel);
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

                return Redirect("/campagins");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError(string.Empty, ex.Message);
                var templates = await _templateService.GetAll();
                var contacts = await _contactService.GetAll();
                var viewModel = new CreateCampaignViewModel
                {
                    Templates = templates,
                    ContactLists = contacts
                };
                return View(viewModel);
            }
        }
        [Route("edit-campaign/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();
            var campaign = await _campaignService.GetCampaignById(id);
            if (campaign == null)
                return NotFound();
            var templates = await _templateService.GetAll();
            var contacts = await _contactService.GetAll();
            var viewModel = new CreateCampaignViewModel
            {
                Campaign = campaign,
                Templates = templates,
                ContactLists = contacts
            };
            return View(viewModel);
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
                return Redirect("/campagins");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var campaign = await _campaignService.GetCampaignById(id);
                var templates = await _templateService.GetAll();
                var contacts = await _contactService.GetAll();
                var viewModel = new CreateCampaignViewModel
                {
                    Campaign = campaign,
                    Templates = templates,
                    ContactLists = contacts
                };
                return View("Edit", viewModel);
            }
        }
        [Route("campaign-report")]
        [HttpGet]
        public async Task<IActionResult> Report()
        {
            var campaigns = await _campaignService.GetAllCampaignNames();
            ViewBag.Campaigns = campaigns;
            return View();
        }

        [Route("campaign-report/stats")]
        [HttpGet]
        public async Task<IActionResult> ReportStats(int? campaignId)
        {
            var data = await _campaignService.GetReportStats(campaignId);
            return Json(data);
        }

        [Route("campaign-report/logs")]
        [HttpGet]
        public async Task<IActionResult> ReportLogs(int? campaignId, int pageIndex = 1, int pageSize = 20)
        {
            var data = await _campaignService.GetReportLogs(campaignId, pageIndex, pageSize);
            return Json(data);
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
