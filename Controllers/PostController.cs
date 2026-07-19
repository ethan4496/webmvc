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
    public class PostController : Controller
    {
        int pageSize = 20;

        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;

        public PostController(ICategoryService categoryService, IPostService postService)
        {
            _categoryService = categoryService;
            _postService = postService;
        }

        [Route("posts")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _postService.GetPaging(new PostSearch { PageIndex = 1, PageSize = pageSize });
            ViewBag.TotalPages = data.TotalPage;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = 1;
            return View(data.Items);
        }

        [HttpGet]
        public async Task<IActionResult> GetPostPaging(PostSearch search)
        {
            search.PageSize = pageSize;
            var data = await _postService.GetPaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_PostTable", data.Items);
        }

        [Route("create-post")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var data = await _categoryService.GetAllCategoryNames();
            ViewBag.Categories = data;
            return View();
        }

        [Route("posts")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostRequest request)
        {
            try
            {
                await _postService.CreateAsync(request);

                TempData["SuccessMessage"] = "Tạo bài viết thành công";

                return Redirect("/posts");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError(string.Empty, ex.Message);
                var data = await _categoryService.GetAllCategoryNames();
                ViewBag.Categories = data;
                return View();
            }
        }
        [Route("post/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();
            var post = await _postService.GetPostById(id);
            if (post == null)
                return NotFound();
            var data = await _categoryService.GetAllCategoryNames();
            ViewBag.Categories = data;
            return View(post);
        }

        [Route("post/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, UpdatePostRequest request)
        {
            try
            {
                await _postService.SaveAsync(id, request);

                TempData["SuccessMessage"] = "Lưu bài viết thành công";

                return Redirect("/posts");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var post = await _postService.GetPostById(id);
                var categories = await _categoryService.GetAllCategoryNames();
                ViewBag.Categories = categories;
                return View(post);
            }
        }

        [Route("posts/delete/{id}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _postService.DeleteAsync(id);
                return Ok(new ApiResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Xóa bài viết thành công",
                    Type = (int)EApiResponseType.Success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Type = (int)EApiResponseType.Error
                });
            }
        }
    }
}
