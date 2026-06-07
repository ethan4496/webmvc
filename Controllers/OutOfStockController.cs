using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using WebMVC.Entities;
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
    public class OutOfStockController : Controller
    {
        int pageSize = 20;

        private readonly IOutOfStockService _outOfStockService;
        private readonly ITransportationService _transportationService;

        public OutOfStockController(IOutOfStockService outOfStockService, ITransportationService transportationService)
        {
            _outOfStockService = outOfStockService;
            _transportationService = transportationService;
        }

        [WebRoleFilter(false)]
        [Route("out-of-stock")]
        public IActionResult Index()
        {
            return View();
        }

        [WebRoleFilter(false)]
        [HttpGet]
        public async Task<IActionResult> GetOutOfStockPaging(OutOfStockSearch search)
        {
            search.PageSize = pageSize;
            var data = await _outOfStockService.GetPaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_OutOfStockTable", data.Items);
        }
        [WebRoleFilter(true)]
        [HttpGet]
        public async Task<IActionResult> GetOutOfStockManagePaging(OutOfStockSearch search)
        {
            search.PageSize = pageSize;
            var data = await _outOfStockService.GetPagingManage(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            ViewBag.TotalItem = data.TotalItem;

            return PartialView("_ManageOutOfStockTable", data.Items);
        }

        [WebRoleFilter(true)]
        [Route("out-of-stock-manage")]
        public IActionResult Manage()
        {
            return View();
        }

        [WebRoleFilter(true)]
        [Route("create-out-of-stock")]
        public IActionResult Create()
        {
            return View();
        }

        [WebRoleFilter(true)]
        [HttpGet]
        public async Task<ApiResponse> GetTransporationAtOutOfStock(int accountId)
        {
            var data = await _transportationService.GetAtOutOfStock(accountId);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [Route("out-of-stock/{id}", Name = "out-of-stock-detail")]
        [WebRoleFilter(true)]
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var data = await _outOfStockService.GetById(id);
            return View(data);
        }

        [HttpGet]
        public async Task<ApiResponse> Print(int id)
        {
            var data = await _outOfStockService.RenderDeliveryNote(new List<int> { id });
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpGet]
        public async Task<ApiResponse> PrintTemp(int id)
        {
            var data = await _outOfStockService.RenderDeliveryNote(new List<int> { id });
            await _outOfStockService.UpdateIsPrintTemp(id);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpGet]
        public async Task<ApiResponse> PrintTempSelected(List<int> ids)
        {
            var data = new StringBuilder();
            foreach (var id in ids)
            {
                data.AppendLine(await _outOfStockService.RenderDeliveryNote(new List<int> { id }));
                await _outOfStockService.UpdateIsPrintTemp(id);
            }
            return new ApiResponse
            {
                Data = data.ToString(),
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpGet]
        public async Task<ApiResponse> PrintAll()
        {
            var data = await _outOfStockService.GetDeliveryNoteUnpaidOfCurrentAccount();
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true)]
        [HttpPut]
        public async Task<ApiResponse> UpdateTransportationAtOutOfStockManage([FromQuery] int id, UpdateTransportationAtOutOfStockManageRequest request)
        {
            await _transportationService.UpdateAtOutOfStockManage(id, request);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true)]
        [HttpPost]
        public async Task<ApiResponse> Export(int id)
        {
            await _outOfStockService.Export(id);
            return new ApiResponse
            {
                Message = "Xuất kho thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true)]
        [HttpPost]
        public async Task<ApiResponse> ExportSelected([FromBody] List<int> ids)
        {
            await _outOfStockService.ExportList(ids);
            return new ApiResponse
            {
                Message = "Xuất kho thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> Cancel(int id)
        {
            await _outOfStockService.Cancel(id);
            return new ApiResponse
            {
                Message = "Hủy phiên thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true)]
        [HttpPut]
        public async Task<ApiResponse> Paied(int id)
        {
            await _outOfStockService.Paied(id);
            return new ApiResponse
            {
                Message = "Xác nhận thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> SendRequest([FromBody] List<int> ids)
        {
            await _outOfStockService.SendRequest(ids);
            return new ApiResponse
            {
                Message = "Xác nhận thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPost]
        public async Task<ApiResponse> SendOutStockNoti([FromBody] List<int> ids)
        {
            await _outOfStockService.SendOutStockNotis(ids);
            return new ApiResponse
            {
                Message = "Thông báo thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true)]
        [HttpPut]
        public async Task<ApiResponse> SendDeliveryNote(int id)
        {
            await _outOfStockService.SendDeliveryNote(id);
            return new ApiResponse
            {
                Message = "Gửi phiếu thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> DeliveryInfo([FromQuery] int id, string deliveryInfo)
        {
            await _outOfStockService.UpdateDeliveryInfo(id, deliveryInfo);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true)]
        [HttpPut]
        public async Task<ApiResponse> TotalPriceVND([FromQuery] int id, decimal totalPriceVND)
        {
            await _outOfStockService.UpdateTotalPriceVND(id, totalPriceVND);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true)]
        [HttpPut]
        public async Task<ApiResponse> PostOffice([FromQuery] int id, string postOffice)
        {
            await _outOfStockService.UpdatePostOffice(id, postOffice);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> DeliveryMethod([FromQuery] int id, string deliveryMethod)
        {
            await _outOfStockService.UpdateDeliveryMethod(id, deliveryMethod);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true)]
        [HttpPost]
        public async Task<ApiResponse> Create(CreateOutOfStockRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }

            var id = await _outOfStockService.Create(request);
            return new ApiResponse
            {
                Message = "Tạo phiên thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
                Data = id
            };
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(OutOfStockSearch search)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Ngày XK";
                worksheet.Cell(1, 3).Value = "PXK";
                worksheet.Cell(1, 4).Value = "Mã vận đơn";
                worksheet.Cell(1, 5).Value = "Ghi chú KH";
                worksheet.Cell(1, 6).Value = "Số kiện";
                worksheet.Cell(1, 7).Value = "Cân nặng ";
                worksheet.Cell(1, 8).Value = "Số khối";
                worksheet.Cell(1, 9).Value = "Tổng cân nặng";
                worksheet.Cell(1, 10).Value = "Tổng số khối";
                worksheet.Cell(1, 11).Value = "Tổng tiền";
                worksheet.Cell(1, 12).Value = "Trạng thái";

                // **Định dạng tiêu đề in đậm**
                //var headerRange = worksheet.Range("A1:F1");
                //headerRange.Style.Font.Bold = true;
                //headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                //headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                search.PageIndex = 1;
                search.PageSize = int.MaxValue;
                var data = await _outOfStockService.GetPaging(search);
                var result = data.Items;
                var transportationsInOutStock = await _outOfStockService.GetTransportationsInOutStock(result.Select(x=>x.Id).ToList());
                // **Ghi dữ liệu vào Excel**
                int stt = 1;
                int row = 2;

                foreach (var item in result)
                {
                    // 🔥 Lấy transportation đúng theo yêu cầu của bạn
                    var transportations = transportationsInOutStock.Where(x=>x.OutOfStockId == item.Id).ToList();

                    // ✅ Nếu KHÔNG có transportation
                    if (!transportations.Any())
                    {
                        worksheet.Cell(row, 1).Value = stt;
                        worksheet.Cell(row, 2).Value = FormatDate.FormatNullDate(item.DateOutStock);
                        worksheet.Cell(row, 3).Value = item.Id;

                        worksheet.Cell(row, 11).Value = item.TotalPriceVND;
                        worksheet.Cell(row, 12).Value = item.StatusPaymentName;

                        row++;
                        stt++;
                        continue;
                    }

                    // ✅ Có transportation
                    int startRow = row;

                    foreach (var t in transportations)
                    {
                        worksheet.Cell(row, 4).Value = t.Barcode;
                        worksheet.Cell(row, 5).Value = t.UserNote;
                        worksheet.Cell(row, 6).Value = t.Quantity;
                        worksheet.Cell(row, 7).Value = t.Weight;
                        worksheet.Cell(row, 8).Value = t.Volume;

                        row++;
                    }

                    int endRow = row - 1;

                    // ✅ Merge cột chung
                    worksheet.Range(startRow, 1, endRow, 1).Merge().Value = stt;
                    worksheet.Range(startRow, 2, endRow, 2).Merge().Value = FormatDate.FormatNullDate(item.DateOutStock);
                    worksheet.Range(startRow, 3, endRow, 3).Merge().Value = item.Id;

                    // 🔥 Fix chuẩn (KHÔNG trùng cột)
                    worksheet.Range(startRow, 9, endRow, 9).Merge().Value = transportations.Sum(x => x.Weight);
                    worksheet.Range(startRow, 10, endRow, 10).Merge().Value = transportations.Sum(x => x.Volume);

                    worksheet.Range(startRow, 11, endRow, 11).Merge().Value = item.TotalPriceVND;
                    worksheet.Range(startRow, 12, endRow, 12).Merge().Value = item.StatusPaymentName;

                    stt++;
                }

                // **Tự động điều chỉnh độ rộng cột**
                worksheet.Columns().AdjustToContents();

                // **Xuất file**
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "data.xlsx");
                }
            }
        }
    }
}
