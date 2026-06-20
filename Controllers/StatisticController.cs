using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Responses;
using WebMVC.Services;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [Authorize]
    public class StatisticController : Controller
    {
        int pageSize = 20;
        private readonly IStatisticService _statisticService;
        private readonly IHttpContextService _httpContextService;

        public StatisticController(IStatisticService statisticService, IHttpContextService httpContextService)
        {
            _statisticService = statisticService;
            _httpContextService = httpContextService;
        }

        [Route("dashboard")]
        [WebRoleFilter(true)]
        public async Task<IActionResult> Dashboard()
        {
            var loggedModel = _httpContextService.GetLoggedModel();
            if (loggedModel.RoleId != (int)ERoleId.User)
            {
                var data = await _statisticService.GetDashboard();
                return View(data);
            }
            else
            {
                return RedirectToAction("Home", "Account");

            }
            
        }

        [HttpGet]
        public async Task<ChartResponse> GetWeekChart(int? id, DateTime? dataDate)
        {
            var result = await _statisticService.WeekStatistic(id, dataDate ?? DateTime.Now);
            return result;
        }

        [HttpGet]
        public async Task<double[]> GetWeekChartData(int? id, DateTime? dataDate, int type)
        {
            var result = await _statisticService.WeekStatisticData(id, dataDate ?? DateTime.Now, type);
            return result;
        }

        [HttpGet]
        public async Task<ChartResponse> GetMonthChart(int? id, DateTime? dataDate)
        {
            var result = await _statisticService.MonthStatistic(id, dataDate ?? DateTime.Now);
            return result;
        }

        [HttpGet]
        public async Task<double[]> GetMonthChartData(int? id, DateTime? dataDate, int type)
        {
            var result = await _statisticService.MonthStatisticData(id, dataDate ?? DateTime.Now, type);
            return result;
        }

        [HttpGet]
        public async Task<ChartResponse> GetYearChart(int? id, DateTime? dataDate)
        {
            var result = await _statisticService.YearStatistic(id, dataDate ?? DateTime.Now);
            return result;
        }

        [HttpGet]
        public async Task<double[]> GetYearChartData(int? id, DateTime? dataDate, int type)
        {
            var result = await _statisticService.YearStatisticData(id, dataDate ?? DateTime.Now, type);
            return result;
        }

        [HttpGet]
        public async Task<List<StatisticTable12Response>> GetStatisticTable1(int? id, DateTime? fromDate, DateTime? toDate)
        {
            var result = await _statisticService.StatisticTable12(id, fromDate, toDate);
            return result.OrderByDescending(x => x.Order).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatisticTable1(int? id, DateTime? fromDate, DateTime? toDate)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Tên khách hàng";
                worksheet.Cell(1, 3).Value = "SĐT";
                worksheet.Cell(1, 4).Value = "Số đơn";
                worksheet.Cell(1, 5).Value = "Tiền thanh toán (VND)";

                // **Định dạng tiêu đề in đậm**
                var headerRange = worksheet.Range("A1:E1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                var result = await _statisticService.StatisticTable12(id, fromDate, toDate);

                // **Ghi dữ liệu vào Excel**
                int row = 2; // Dòng bắt đầu từ 2 (sau tiêu đề)
                int stt = 1; // Số thứ tự
                foreach (var item in result.OrderByDescending(x => x.Order).ToList())
                {
                    worksheet.Cell(row, 1).Value = stt++; // STT
                    worksheet.Cell(row, 2).Value = item.Username; // Tên khách hàng
                    worksheet.Cell(row, 3).Value = item.Phone; // SĐT
                    worksheet.Cell(row, 4).Value = item.Order; // Số đơn
                    worksheet.Cell(row, 5).Value = item.TotalPriceVND; // Tổng tiền

                    // **Định dạng số tiền**
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0"; // Hiển thị có dấu phẩy phân cách

                    row++;
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
                        "Khách hàng có nhiều đơn trong tháng.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<List<StatisticTable12Response>> GetStatisticTable2(int? id, int? shipId, DateTime? fromDate, DateTime? toDate)
        {
            var result = await _statisticService.StatisticTable12(id, fromDate, toDate, shipId);
            return result.OrderByDescending(x => x.TotalPriceVND).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatisticTable2(int? id, DateTime? fromDate, DateTime? toDate, int? shipId)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Tên khách hàng";
                worksheet.Cell(1, 3).Value = "SĐT";
                worksheet.Cell(1, 5).Value = "Tiền thanh toán (VND)";

                // **Định dạng tiêu đề in đậm**
                var headerRange = worksheet.Range("A1:D1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                var result = await _statisticService.StatisticTable12(id, fromDate, toDate, shipId);

                // **Ghi dữ liệu vào Excel**
                int row = 2; // Dòng bắt đầu từ 2 (sau tiêu đề)
                int stt = 1; // Số thứ tự
                foreach (var item in result.OrderByDescending(x => x.TotalPriceVND).ToList())
                {
                    worksheet.Cell(row, 1).Value = stt++; // STT
                    worksheet.Cell(row, 2).Value = item.Username; // Tên khách hàng
                    worksheet.Cell(row, 3).Value = item.Phone; // SĐT
                    worksheet.Cell(row, 5).Value = item.TotalPriceVND; // Tổng tiền

                    // **Định dạng số tiền**
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0"; // Hiển thị có dấu phẩy phân cách

                    row++;
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
                        "Doanh số trong tháng.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<StatisticTable3TitleResponse> GetStatisticTitleTable3(int? id, DateTime? fromDate, DateTime? toDate)
        {
            var result = await _statisticService.StatisticTitleTable3(id, fromDate, toDate);
            return result;
        }

        [HttpGet]
        public async Task<List<StatisticTable3Response>> GetStatisticTable3(int? id, DateTime? fromDate, DateTime? toDate, bool? isOrder)
        {
            var result = await _statisticService.StatisticTable3(id, fromDate, toDate, isOrder);
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatisticTable3(int? id, DateTime? fromDate, DateTime? toDate, bool? isOrder)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Id";
                worksheet.Cell(1, 3).Value = "Đã mua hàng";
                worksheet.Cell(1, 4).Value = "Username";
                worksheet.Cell(1, 5).Value = "SĐT";
                worksheet.Cell(1, 6).Value = "Ngày tạo";

                // **Định dạng tiêu đề in đậm**
                var headerRange = worksheet.Range("A1:F1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                var result = await _statisticService.StatisticTable3(id, fromDate, toDate, isOrder);

                // **Ghi dữ liệu vào Excel**
                int row = 2; // Dòng bắt đầu từ 2 (sau tiêu đề)
                int stt = 1; // Số thứ tự
                foreach (var item in result)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = item.Id;
                    worksheet.Cell(row, 3).Value = item.IsOrder ? "Đã đặt" : "";
                    worksheet.Cell(row, 4).Value = item.Username;
                    worksheet.Cell(row, 5).Value = item.Phone;
                    worksheet.Cell(row, 6).Value = item.Created;
                    row++;
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
                        "Khách hàng mới trong tháng.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<List<StatisticTable4Response>> GetStatisticTable4(int? id, DateTime? fromDate, DateTime? toDate)
        {
            var result = await _statisticService.StatisticTable4(id, fromDate, toDate);
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatisticTable4(int? id, DateTime? fromDate, DateTime? toDate)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Username";
                worksheet.Cell(1, 3).Value = "SĐT";
                worksheet.Cell(1, 4).Value = "Tổng tiền";

                // **Định dạng tiêu đề in đậm**
                var headerRange = worksheet.Range("A1:D1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                var result = await _statisticService.StatisticTable4(id, fromDate, toDate);

                // **Ghi dữ liệu vào Excel**
                int row = 2; // Dòng bắt đầu từ 2 (sau tiêu đề)
                int stt = 1; // Số thứ tự
                foreach (var item in result)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = item.Username;
                    worksheet.Cell(row, 3).Value = item.Phone;
                    worksheet.Cell(row, 4).Value = item.TotalPriceVND;
                    row++;
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
                        "Khách hàng giao dịch lần đầu.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<List<StatisticTabl56Response>> GetStatisticTable5(int? id)
        {
            var result = await _statisticService.StatisticTable5(id);
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatisticTable5(int? id)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Username";
                worksheet.Cell(1, 3).Value = "SĐT";
                worksheet.Cell(1, 4).Value = "Đơn hàng cuối";
                worksheet.Cell(1, 5).Value = "Ngày chênh lệch";

                // **Định dạng tiêu đề in đậm**
                var headerRange = worksheet.Range("A1:E1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                var result = await _statisticService.StatisticTable5(id);

                // **Ghi dữ liệu vào Excel**
                int row = 2; // Dòng bắt đầu từ 2 (sau tiêu đề)
                int stt = 1; // Số thứ tự
                foreach (var item in result)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = item.Username;
                    worksheet.Cell(row, 3).Value = item.Phone;
                    worksheet.Cell(row, 4).Value = item.LastOrder;
                    worksheet.Cell(row, 5).Value = item.DifferenceDay > 0 ? item.DifferenceDay : "";
                    row++;
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
                        "Khách chưa đi hàng trong 6 tháng.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<List<StatisticTabl56Response>> GetStatisticTable6(int? id)
        {
            var result = await _statisticService.StatisticTable6(id);
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatisticTable6(int? id)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Username";
                worksheet.Cell(1, 3).Value = "SĐT";
                worksheet.Cell(1, 4).Value = "Đơn hàng cuối";
                worksheet.Cell(1, 5).Value = "Ngày chênh lệch";

                // **Định dạng tiêu đề in đậm**
                var headerRange = worksheet.Range("A1:E1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                var result = await _statisticService.StatisticTable6(id);

                // **Ghi dữ liệu vào Excel**
                int row = 2; // Dòng bắt đầu từ 2 (sau tiêu đề)
                int stt = 1; // Số thứ tự
                foreach (var item in result)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = item.Username;
                    worksheet.Cell(row, 3).Value = item.Phone;
                    worksheet.Cell(row, 4).Value = item.LastOrder;
                    worksheet.Cell(row, 5).Value = item.DifferenceDay > 0 ? item.DifferenceDay : "";
                    row++;
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
                        "Khách chưa đi hàng trong 3 tháng.xlsx");
                }
            }
        }

        [Route("report-chart")]
        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        public IActionResult ReportChart()
        {
            return View();
        }

        [HttpGet]
        public async Task<ColumnChartResponse> GetReportFeeYearChartData(DateTime? dataDate)
        {
            var result = await _statisticService.ReportFeeYearChartData(dataDate ?? DateTime.Now);
            return result;
        }

        [HttpGet]
        public async Task<ColumnChartResponse> GetReportFeePostOfficeChartData(DateTime? dataDate)
        {
            var result = await _statisticService.ReportFeePostOfficeChartData(dataDate ?? DateTime.Now);
            return result;
        }

        [HttpGet]
        public async Task<ColumnChartResponse> GetReportNewAccountChartData(DateTime? dataDate)
        {
            var result = await _statisticService.ReportNewAccountChartData(dataDate ?? DateTime.Now);
            return result;
        }

        [HttpGet]
        public async Task<ColumnChartResponse> GetReportAccountOrderedChartData(DateTime? dataDate)
        {
            var result = await _statisticService.ReportAccountOrderedChartData(dataDate ?? DateTime.Now);
            return result;
        }

        [Route("report-revenue")]
        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        public IActionResult ReportRevenue(int? type)
        {
            return View(type);
        }

        [HttpGet]
        public async Task<IActionResult> GetReportRevenue(ReportRevenueSearch search)
        {
            search.PageSize = pageSize;
            var data = await _statisticService.GetReportRevenuePaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_ReportRevenueTable", data.Items);
        }

        [HttpGet]
        public async Task<ChartResponse> GetReportRevenueYearChart(ReportRevenueSearch search)
        {
            var result = await _statisticService.ReportRevenueYearStatistic(search);
            return result;
        }

        [HttpGet]
        public async Task<double[]> GetReportRevenueYearStatisticData(ReportRevenueSearch search)
        {
            var result = await _statisticService.ReportRevenueYearStatisticData(search);
            return result;
        }

        [Route("report-fixed-fee")]
        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        public IActionResult ReportFixedFee(int? type)
        {
            return View(type);
        }

        [HttpGet]
        public async Task<IActionResult> GetReportFixedFee(ReportFeeSearch search)
        {
            search.PageSize = pageSize;
            var data = await _statisticService.GetReportFixedFeePaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_ReportFixedFeeTable", data.Items);
        }

        [Route("report-other-fee")]
        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        public IActionResult ReportOtherFee(int? type)
        {
            return View(type);
        }

        [HttpGet]
        public async Task<IActionResult> GetReportOtherFee(ReportFeeSearch search)
        {
            search.PageSize = pageSize;
            var data = await _statisticService.GetReportOtherFeePaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_ReportOtherFeeTable", data.Items);
        }

        [Route("report-partner-fee")]
        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        public IActionResult ReportPartnerFee(int? type)
        {
            return View(type);
        }

        [HttpGet]
        public async Task<IActionResult> GetReportPartnerFee(ReportFeeSearch search)
        {
            search.PageSize = pageSize;
            var data = await _statisticService.GetReportPartnerFeePaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_ReportPartnerFeeTable", data.Items);
        }

        [HttpGet]
        public async Task<IActionResult> ExportReportPartnerFee(ReportFeeSearch search)
        {
            search.PageIndex = 1;
            search.PageSize = int.MaxValue;
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Tài khoản nhập";
                worksheet.Cell(1, 3).Value = "Ngày";
                worksheet.Cell(1, 4).Value = "Mã hàng";
                worksheet.Cell(1, 5).Value = "Cân nặng";
                worksheet.Cell(1, 6).Value = "Số khối";
                worksheet.Cell(1, 7).Value = "Số tiền";
                worksheet.Cell(1, 8).Value = "Ghi chú";

                // **Định dạng tiêu đề in đậm**
                var headerRange = worksheet.Range("A1:H1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                var result = await _statisticService.GetReportPartnerFeePaging(search);
                if (result.Items.Any())
                {
                    // **Ghi dữ liệu vào Excel**
                    int row = 2; // Dòng bắt đầu từ 2 (sau tiêu đề)
                    int stt = 1; // Số thứ tự
                    foreach (var item in result.Items)
                    {
                        worksheet.Cell(row, 1).Value = stt++;
                        worksheet.Cell(row, 2).Value = item.Username;
                        worksheet.Cell(row, 3).Value = item.DataDate.ToString("dd/MM/yyyy");
                        worksheet.Cell(row, 4).Value = item.Code;
                        worksheet.Cell(row, 5).Value = item.Weight;
                        worksheet.Cell(row, 6).Value = item.Volume;
                        worksheet.Cell(row, 7).Value = item.Amount;
                        worksheet.Cell(row, 8).Value = item.Note;
                        row++;
                    }
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
                        "Khách chưa đi hàng trong 3 tháng.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<decimal[]> GetReportFixedFeeYearStatisticData(ReportFeeSearch search)
        {
            var result = await _statisticService.ReportFixedFeeYearStatisticData(search);
            return result;
        }

        [HttpGet]
        public async Task<decimal[]> GetReportOtherFeeYearStatisticData(ReportFeeSearch search)
        {
            var result = await _statisticService.ReportOtherFeeYearStatisticData(search);
            return result;
        }

        [HttpGet]
        public async Task<ChartResponse> GetReportPartnerFeeYearStatistic(ReportFeeSearch search)
        {
            var result = await _statisticService.ReportPartnerFeeYearStatistic(search);
            return result;
        }

        [HttpGet]
        public async Task<double[]> GetReportPartnerFeeYearStatisticData(ReportFeeSearch search)
        {
            var result = await _statisticService.ReportPartnerFeeYearStatisticData(search);
            return result;
        }

        [HttpPost]
        public async Task<ApiResponse> CreateFixedFee(CreateFixedFeeRequest request)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            await _statisticService.CreateReportFixedFee(request);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Tạo thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPost]
        public async Task<ApiResponse> CreateOtherFee(CreateOtherFeeRequest request)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            await _statisticService.CreateReportOtherFee(request);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Tạo thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPost]
        public async Task<ApiResponse> CreatePartnerFee(CreatePartnerFeeRequest request)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            await _statisticService.CreateReportPartnerFee(request);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Tạo thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpGet]
        public IActionResult ExportSampleExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Mẫu");
                worksheet.Cell(1, 1).Value = "Mã hàng"; // Thêm tiêu đề cột
                worksheet.Cell(1, 2).Value = "Cân nặng"; // Thêm tiêu đề cột
                worksheet.Cell(1, 3).Value = "Số khối"; // Thêm tiêu đề cột
                worksheet.Cell(1, 4).Value = "Số tiền"; // Thêm tiêu đề cột
                worksheet.Cell(1, 5).Value = "Ghi chú"; // Thêm tiêu đề cột
                worksheet.Cell(1, 6).Value = "Ngày tạo"; // Thêm tiêu đề cột
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
        public async Task<IActionResult> ImportPartnerFee([FromQuery] int? postOffice, [FromForm] IFormFile file)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Vui lòng chọn file hợp lệ.",
                    Type = (int)EApiResponseType.Error
                });
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                    var lastColumn = worksheet.LastColumnUsed().ColumnNumber(); // Cột cuối cùng có dữ liệu
                    int resultColumn = lastColumn + 1;

                    // Thêm tiêu đề cho cột kết quả import
                    worksheet.Cell(1, resultColumn).Value = $"Kết quả Import";
                    worksheet.Cell(1, resultColumn).Style.Font.Bold = true;

                    int rowCount = worksheet.LastRowUsed().RowNumber();
                    for (int row = 2; row <= rowCount; row++)
                    {
                        bool success = true;
                        string failedMessage = "";
                        try
                        {
                            string code = worksheet.Cell(row, 1).GetString().Trim();
                            string weight = worksheet.Cell(row, 2).GetString().Trim();
                            string volume = worksheet.Cell(row, 3).GetString().Trim();
                            string totalPriceVND = worksheet.Cell(row, 4).GetString().Trim().Replace(",", "");
                            string note = worksheet.Cell(row, 5).GetString().Trim();
                            DateTime.TryParseExact(worksheet.Cell(row, 6).GetString().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataDate);
                            var importData = new CreatePartnerFeeRequest
                            {
                                Code = code,
                                Amount = decimal.Parse(totalPriceVND != "" ? totalPriceVND : "0"),
                                Weight = double.Parse(weight != "" ? weight : "0"),
                                Volume = double.Parse(volume != "" ? volume : "0"),
                                Note = note,
                                PostOffice = postOffice,
                                DataDate = dataDate
                            };

                            await _statisticService.CreateReportPartnerFee(importData);
                        }
                        catch (Exception ex)
                        {
                            success = false;
                            failedMessage = ex.Message;
                        }
                        worksheet.Cell(row, 1).Style.NumberFormat.Format = "@";
                        worksheet.Cell(row, resultColumn).Value = success ? "Thành công" : failedMessage;
                        worksheet.Cell(row, resultColumn).Style.Fill.BackgroundColor = success ? XLColor.LightGreen : XLColor.LightPink;
                    }

                    // **Tự động điều chỉnh độ rộng tất cả các cột**
                    worksheet.Columns().AdjustToContents();

                    // Trả về file kết quả cho người dùng
                    using (var outputStream = new MemoryStream())
                    {
                        workbook.SaveAs(outputStream);
                        outputStream.Position = 0;
                        return File(outputStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ImportResult.xlsx");
                    }
                }
            }
        }

        [HttpPut]
        public async Task<ApiResponse> UpdateFixedFee([FromQuery] int id, CreateFixedFeeRequest request)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            await _statisticService.UpdateReportFixedFee(id, request);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Cập nhật thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPut]
        public async Task<ApiResponse> UpdateOtherFee([FromQuery] int id, CreateOtherFeeRequest request)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            await _statisticService.UpdateReportOtherFee(id, request);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Cập nhật thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPut]
        public async Task<ApiResponse> UpdatePartnerFee([FromQuery] int id, CreatePartnerFeeRequest request)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            await _statisticService.UpdateReportPartnerFee(id, request);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Cập nhật thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPut]
        public async Task<ApiResponse> AcceptFixedFee([FromQuery] int id)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            await _statisticService.AcceptReportFixedFee(id);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Duyệt thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPut]
        public async Task<ApiResponse> AcceptOtherFee([FromQuery] int id)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            await _statisticService.AcceptReportOtherFee(id);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Duyệt thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpDelete]
        public async Task<ApiResponse> DeletePartnerFee([FromQuery] int id)
        {
            if (!ModelState.IsValid)
                throw new AppException("ModelState invalid");
            await _statisticService.DeleteReportPartnerFee(id);
            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Duyệt thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpGet]
        public async Task<ChartResponse> GetYearChartOfAccount(int id)
        {
            var result = await _statisticService.YearStatisticOfAccount(id, DateTime.Now);
            return result;
        }

        [HttpGet]
        public async Task<YearStatisticDataOfAccountResponse> GetYearChartDataOfAccount(int id, int type)
        {
            var result = await _statisticService.YearStatisticDataOfAccount(id, DateTime.Now, type);
            return result;
        }
    }
}
