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
    [Authorize]
    [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff)]
    public class PricingController : Controller
    {
        int pageSize = 20;

        private readonly IPricingService _pricingService;
        private readonly IWarehouseService _warehouseService;

        public PricingController(IPricingService pricingService, IWarehouseService warehouseService)
        {
            _pricingService = pricingService;
            _warehouseService = warehouseService;
        }

        [Route("pricing")]
        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseService.GetWarehousesByStatus(-1);
            return View(warehouses);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaging(PricingSearch search)
        {
            search.PageSize = pageSize;
            var data = await _pricingService.GetPaging(search);
            ViewBag.TotalPages = data.TotalPage;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = 1;
            return PartialView("_PricingTable", data.Items);
        }

        [HttpPost]
        public async Task<ApiResponse> Create(Pricing request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _pricingService.Create(request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Tạo thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPut]
        public async Task<ApiResponse> Update([FromQuery] int id, UpdatePricingRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _pricingService.Update(id, request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Cập nhật thành công",
                Type = (int)EApiResponseType.Success,
            };
        }


        [HttpDelete]
        public async Task<ApiResponse> Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _pricingService.Delete(id);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Xóa thành công",
                Type = (int)EApiResponseType.Success,
            };
        }
    }
}
