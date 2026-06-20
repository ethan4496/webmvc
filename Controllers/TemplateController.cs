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
    
    public class TemplateController : Controller
    {
        private readonly ITemplateService _templateService;
        public TemplateController(ITemplateService templateService, IWarehouseService warehouseService)
        {
            _templateService = templateService;
        }

        [Route("templates")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplatePaging(TemplateSearch search)
        {
            var data = await _templateService.GetPaging(search);
            return Json(new
            {
                items = data.Items,
                currentPage = search.PageIndex,
                pageSize = search.PageSize,
                totalPages = data.TotalPage,
                totalItems = data.TotalItem
            });
        }

        
        [Route("create-template")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        
        [Route("templates")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTemplateRequest request)
        {
            try
            {
                await _templateService.CreateAsync(request);

                TempData["SuccessMessage"] = "Tạo template thành công";

                return Redirect("/templates");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(request);
            }
        }
        [Route("edit-template/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();
            var template = await _templateService.GetTemplateById(id);
            if (template == null)
                return NotFound();
            return View(template);
        }

        [Route("edit-template/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, CreateTemplateRequest request)
        {
            try
            {
                await _templateService.SaveAsync(id, request);
                TempData["SuccessMessage"] = "Save template thành công";
                return Redirect("/templates");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var template = await _templateService.GetTemplateById(id);
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
            await _templateService.DeleteAsync(id);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Xóa thành công",
                Type = (int)EApiResponseType.Success,
            };
        }
    }
}
