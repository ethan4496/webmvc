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

        [Route("contact-detail/{id}")]
        [HttpGet]
        public async Task<IActionResult> Detail(int id, string email)
        {
             if (id <= 0)
                return BadRequest();
            var contact = await _contactService.GetContactById(id, email);
            if (contact == null)
                return NotFound();

            return View(contact);
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

                return Redirect("/contact-lists");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(request);
            }
        }

        [Route("edit-contact/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, AddContactListRequest request)
        {
            try
            {
                await _contactService.SaveAsync(id, request);
                TempData["SuccessMessage"] = "Save template thành công";
                return Redirect("/contact-lists");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var contact = await _contactService.GetContactById(id);
                return View("Detail", contact);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddContact(AddContactRequest request)
        {
            try
            {
                // Console.WriteLine(
                //     "AddContact"
                // );
                var contact = await _contactService.addContact(request);
                return Ok(new
                {
                    success = true,
                    data = new { contact.Id, contact.FirstName, contact.LastName, contact.Email },
                    message = "Thêm liên hệ thành công"
                });
            }
            catch (Exception ex)
            {
                // Console.WriteLine(
                //     "items"
                // );
                // Console.WriteLine(
                //     ex.ToString()
                // );
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpPut]
        public async Task<IActionResult> EditContact( [FromBody] UpdateContactRequest request)
        {
            try
            {
                await _contactService.EditContactAsync(request);
                return Ok(new { success = true, message = "Cập nhật liên hệ thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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
        [HttpDelete]
        public async Task<ApiResponse> DeleteContact(int id, int ContactListId)
        {
            
            await _contactService.DeleteContact(id, ContactListId);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Xóa thành công",
                Type = (int)EApiResponseType.Success,
            };
        }
    }
}
