using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Services;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    public class TrackingController : Controller
    {
        private readonly ITrackingService _trackingService;

        public TrackingController(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }
        [Route("tracking")]
        public async Task<IActionResult> Index(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
            {
                return View(null);
            }

            var data = await _trackingService.TrackingByBarcode(barcode);
            return View(data);
        }

        public async Task<IActionResult> IndexContent(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
            {
                return PartialView(new TrackingResponse());
            }
            var data = await _trackingService.TrackingByBarcode(barcode);
            if (data == null)
                data = new TrackingResponse();
            return PartialView(data);
        }

        [HttpPost]
        public async Task<JsonResult> CalculateTracking([FromBody] CreateTrackingRequest request)
        {
            return Json(await _trackingService.Create(request));
        }

        [HttpGet]
        public async Task<JsonResult> GetTracking(string barcode)
        {
            return Json(await _trackingService.GetByBarcode(barcode));
        }

        [HttpPost]
        public async Task<ApiResponse> UpdateProduct([FromForm] UpdateTransportationProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _trackingService.UpdateTransportationProduct(request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Cập nhật thành công",
                Type = (int)EApiResponseType.Success,
            };
        }
    }
}
