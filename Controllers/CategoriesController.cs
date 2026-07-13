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
    public class CategoriesController : Controller
    {
        int pageSize = 20;

        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Route("categories")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _categoryService.GetPaging(new CategorySearch { PageIndex = 1, PageSize = pageSize });
            ViewBag.TotalPages = data.TotalPage;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = 1;
            return View(data.Items);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryPaging(CategorySearch search)
        {
            search.PageSize = pageSize;
            var data = await _categoryService.GetPaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_CategoryTable", data.Items);
        }

        [HttpGet]
        public async Task<ApiResponse> GetAllCategoryNames()
        {
            var data = await _categoryService.GetAllCategoryNames();
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
                Data = data
            };
        }

        [HttpPost]
        public async Task<ApiResponse> Create([FromForm] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _categoryService.CreateAsync(request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Tạo danh mục thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPut]
        public async Task<ApiResponse> Update([FromQuery] int id, [FromForm] UpdateCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _categoryService.SaveAsync(id, request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Cập nhật danh mục thành công",
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
            await _categoryService.DeleteAsync(id);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Xóa thành công",
                Type = (int)EApiResponseType.Success,
            };
        }
    }
}
