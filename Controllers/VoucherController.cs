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
    [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff)]
    public class VoucherController : Controller
    {
        int pageSize = 20;

        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService VoucherService)
        {
            _voucherService = VoucherService;
        }
        [Route("voucher")]
        public async Task<IActionResult> Index()
        {
            var data = await _voucherService.GetPaging(new VoucherSearch { PageIndex = 1, PageSize = pageSize });
            ViewBag.TotalPages = data.TotalPage;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = 1;
            return View(data.Items);
        }

        [HttpGet]
        public async Task<IActionResult> GetVoucherPaging(VoucherSearch search)
        {
            search.PageSize = pageSize;
            var data = await _voucherService.GetPaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_VoucherTable", data.Items);
        }

        [Route("voucher-detail/{id}", Name = "voucher-detail")]
        public async Task<IActionResult> Detail(int id)
        {
            var data = await _voucherService.GetVoucherAccountDetail(id);
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetVoucherAccountPaging(VoucherAccountSearch search)
        {
            search.PageSize = pageSize;
            var data = await _voucherService.GetVoucherAccountPaging(search);
            ViewBag.TotalPages = data.TotalPage;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = 1;
            return PartialView("_VoucherAccountTable", data.Items);
        }

        [HttpGet]
        public async Task<ApiResponse> GetVoucherAccount()
        {
            var data = await _voucherService.GetVoucherAccountByCurrentAccountId();
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
                Data = data
            };
        }


        [HttpGet]
        public IActionResult ExportSampleExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Mẫu");
                worksheet.Cell(1, 1).Value = "Username"; // Thêm tiêu đề cột
                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sample.xlsx");
                }
            }
        }

        [HttpPost]
        public async Task<ApiResponse> Create([FromForm] CreateVoucherRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _voucherService.Create(request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Tạo voucher thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPut]
        public async Task<ApiResponse> Update([FromQuery] int id, [FromForm] UpdateVoucherRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _voucherService.Update(id, request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Cập nhật voucher thành công",
                Type = (int)EApiResponseType.Success,
            };
        }


        [HttpPut]
        public async Task<ApiResponse> Recall(int id)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _voucherService.RecallAccountVoucher(id);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Thu hồi voucher thành công",
                Type = (int)EApiResponseType.Success,
            };
        }
    }
}
