using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebMVC.Entities;
using WebMVC.Interfaces;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [Authorize]
    public class StaffTargetController : Controller
    {
        private readonly IStaffTargetService _staffTargetService;

        public StaffTargetController(IStaffTargetService staffTargetService)
        {
            _staffTargetService = staffTargetService;
        }

        [Route("staff-target")]
        public async Task<IActionResult> Index(DateTime? dataDate)
        {
            var model = await _staffTargetService.GetListStaffTarget(dataDate ?? DateTime.Now);
            return View(model);
        }

        [WebRoleFilter(true, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff)]
        [Route("staff-target-detail/{id}", Name = "staff-target-detail")]
        public ActionResult Detail(int id)
        {
            return View(id);
        }

        public async Task<IActionResult> GetStaffTarget(int id, DateTime? dataDate)
        {
            var model = await _staffTargetService.GetListStaffTarget(dataDate ?? DateTime.Now, id);
            return PartialView("_StaffTargetTable", model.FirstOrDefault());
        }
        [WebRoleFilter(true)]
        [HttpPut]
        public async Task<ApiResponse> Update([FromQuery] int id, StaffTarget request)
        {
            await _staffTargetService.UpdateStaffTarget(id, request);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true)]
        [HttpPut]
        public async Task<ApiResponse> StaffImage([FromQuery] int id, IFormFile image)
        {
            await _staffTargetService.UpdateStaffImage(id, image);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }
    }
}
