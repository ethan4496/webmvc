using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [WebRoleFilter(true)]
    [Authorize]
    public class BigPackageController : Controller
    {
        private readonly IBigPackageService _bigPackageService;
        int pageSize = 20;
        public BigPackageController(IBigPackageService bigPackageService)
        {
            _bigPackageService = bigPackageService;
        }
        [Route("big-package")]
        public async Task<IActionResult> Index()
        {
            var data = await _bigPackageService.GetPaging(new BigPackageSearch { PageIndex = 1, PageSize = pageSize });
            ViewBag.TotalPages = data.TotalPage;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = 1;
            return View(data.Items);
        }

        [HttpGet]
        public async Task<IActionResult> GetBigPackagePaging(BigPackageSearch search)
        {
            search.PageSize = pageSize;
            var data = await _bigPackageService.GetPaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_BigPackageTable", data.Items);
        }

        [HttpGet]
        [Route("big-package/{id}", Name = "big-package-detail")]
        public async Task<IActionResult> Detail(int id)
        {
            var data = await _bigPackageService.GetById(id);
            return View(data);
        }

        [HttpPut]
        public async Task<ApiResponse> Update([FromQuery] int id, [FromBody] UpdateBigPackageRequest request)
        {
            await _bigPackageService.Update(id, request);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        [HttpDelete]
        public async Task<ApiResponse> DeleteSelected([FromBody] List<int> ids)
        {
            await _bigPackageService.DeleteSelected(ids);
            return new ApiResponse
            {
                Message = "Xóa thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        [HttpDelete]
        public async Task<ApiResponse> DeleteFilterd([FromBody] BigPackageSearch search)
        {
            await _bigPackageService.DeleteFilterd(search);
            return new ApiResponse
            {
                Message = "Xóa thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        #region Json data
        [HttpGet]
        public async Task<IActionResult> Paging(BigPackageSearch search)
        {
            var data = await _bigPackageService.GetPaging(search);
            return Json(data);
        }
        #endregion
    }
}
