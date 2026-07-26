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
using WebMVC.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WebMVC.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebMVC.Controllers
{
    [Authorize]

    public class CampaignController : Controller
    {
        private readonly ICampaignService _campaignService;
        private readonly ITemplateService _templateService;
        private readonly IContactService _contactService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDataProtector _trackingProtector;

        public CampaignController(ICampaignService campaginService, ITemplateService templateService, IContactService contactService, IServiceScopeFactory scopeFactory, IDataProtectionProvider dataProtectionProvider)
        {
            _campaignService = campaginService;
            _templateService = templateService;
            _contactService = contactService;
            _scopeFactory = scopeFactory;
            _trackingProtector = dataProtectionProvider.CreateProtector("EmailTracking.Open");
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

        [Route("email-tracking")]
        [HttpGet]
        public async Task<IActionResult> EmailTrackingIndex()
        {
            var campaigns = await _campaignService.GetAllCampaignNames();
            ViewBag.Campaigns = campaigns;
            return View();
        }

        [Route("email-tracking/list")]
        [HttpGet]
        public async Task<IActionResult> EmailTrackingList(int? campaignId, int pageIndex = 1, int pageSize = 20)
        {
            var data = await _campaignService.GetEmailTrackingList(campaignId, pageIndex, pageSize);
            return Json(data);
        }

        [Route("campaign-report/export-tracking")]
        [HttpGet]
        public async Task<IActionResult> ExportTracking(int campaignId)
        {
            var db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            var campaign = await db.Campaigns.FindAsync(campaignId);
            var data = await db.EmailTrackings
                .Where(x => x.CampaignId == campaignId)
                .OrderByDescending(x => x.OpenCount)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("EmailTracking");

                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Email";
                worksheet.Cell(1, 3).Value = "Campaign Name";
                worksheet.Cell(1, 4).Value = "Email Sent";
                worksheet.Cell(1, 5).Value = "Created At";

                int row = 2;
                int stt = 1;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = stt;
                    worksheet.Cell(row, 2).Value = item.RecipientEmail;
                    worksheet.Cell(row, 3).Value = campaign?.Name;
                    worksheet.Cell(row, 4).Value = campaign?.EmailSent;
                    worksheet.Cell(row, 5).Value = item.CreatedAt;
                    row++;
                    stt++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"email-tracking-campaign-{campaignId}.xlsx");
                }
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

        [AllowAnonymous]
        [HttpGet("api/track/gen")]
        public IActionResult TrackGen(int campaignId, string email)
        {
            var token = WebEncoders.Base64UrlEncode(_trackingProtector.Protect(Encoding.UTF8.GetBytes($"{campaignId}:{email}")));
            return Ok(new { trackingId = token });
        }

        [AllowAnonymous]
        [HttpGet("api/track/open/{trackingId}")]
        public IActionResult TrackOpen(string trackingId)
        {
            using var scope = _scopeFactory.CreateScope();
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            int campaignId = 0;
            string email = null;
            try
            {
                var payload = Encoding.UTF8.GetString(_trackingProtector.Unprotect(WebEncoders.Base64UrlDecode(trackingId)));
                var parts = payload.Split(':', 2);
                campaignId = int.Parse(parts[0]);
                email = parts[1];
            }
            catch
            {
                // Token không hợp lệ: vẫn trả ảnh, chỉ bỏ qua việc ghi nhận open
            }

            if (!string.IsNullOrEmpty(email))
            {
                var record = _db.EmailTrackings.FirstOrDefault(x => x.CampaignId == campaignId && x.RecipientEmail == email);
                if (record == null)
                {
                    record = new EmailTracking
                    {
                        CampaignId = campaignId,
                        RecipientEmail = email,
                        OpenCount = 0,
                    };
                    _db.EmailTrackings.Add(record);
                }
                _db.SaveChanges();
            }

            // Trả về ảnh gif 1x1 trong suốt
            var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==");
            return File(pixel, "image/gif");
        }
    }
}
