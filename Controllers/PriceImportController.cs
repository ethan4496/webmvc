using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Services;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    public class PriceImportController : Controller
    {
        private readonly IPricingImportService _pricingImportService;

        public PriceImportController(IPricingImportService pricingImportService)
        {
            _pricingImportService = pricingImportService;
        }

        [Route("price-import")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ApiResponse> Create(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // đọc excel
            }
            try
            {
                await _pricingImportService.ImportExcelAsync(file);

                return new ApiResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Import thành công",
                    Type = (int)EApiResponseType.Success
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.InnerException?.Message ?? ex.Message,
                    Type = (int)EApiResponseType.Error
                };
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] PriceImportFilter filter)
        {
            var data = await _pricingImportService.GetListAsync(filter);

            return Ok(data);
        }

        [HttpPost]
        public async Task<ApiResponse> Update(PriceImport model)
        {
            await _pricingImportService.UpdateAsync(model);

            return new ApiResponse
            {
                StatusCode = 200,
                Message = "Cập nhật thành công",
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPost]
        public async Task<ApiResponse> Delete(int id)
        {
            await _pricingImportService.DeleteAsync(id);

            return new ApiResponse
            {
                StatusCode = 200,
                Message = "Xóa thành công",
                Type = (int)EApiResponseType.Success
            };
        }
    }
}