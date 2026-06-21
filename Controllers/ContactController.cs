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
    
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;
        private readonly ITemplateService _templateService;
        public ContactController(IContactService contactService, ITemplateService templateService)
        {
            _contactService = contactService;
            _templateService = templateService;
        }

        [Route("contact-lists")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetContactListPaging(ContactSearch search)
        {
            var data = await _contactService.GetPaging(search);
            return Json(new
            {
                items = data.Items,
                currentPage = search.PageIndex,
                pageSize = search.PageSize,
                totalPages = data.TotalPage,
                totalItems = data.TotalItem
            });
        }

        
        [Route("create-contact-list")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        
        [Route("contact-lists")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContactListRequest request)
        {
            try
            {
                await _contactService.CreateAsync(request);

                TempData["SuccessMessage"] = "Tạo contact list thành công";

                return Redirect("/templates");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(request);
            }
        }

        [Route("add-contact/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, AddContactRequest request)
        {
            try
            {
                await _contactService.addContact(id, request);
                TempData["SuccessMessage"] = "Save template thành công";
                return Redirect("/templates");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var template = await _contactService.GetCampaignById(id);
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
            await _contactService.DeleteAsync(id);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Xóa thành công",
                Type = (int)EApiResponseType.Success,
            };
        }
    }
}
