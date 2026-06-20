using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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
    public class TransportationController : Controller
    {
        int pageSize = 40;

        private readonly ITransportationService _transportationService;
        private readonly IWarehouseService _warehouseService;

        public TransportationController(ITransportationService transportationService, IWarehouseService warehouseService)
        {
            _transportationService = transportationService;
            _warehouseService = warehouseService;
        }

        [Route("transportation")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseService.GetWarehousesByType((int)EWarehouseType.Shipping);

            return View(warehouses);
        }

        [HttpGet]
        public async Task<IActionResult> GetTransportationPaging(TransportationSearch search)
        {
            search.PageSize = pageSize;
            var data = await _transportationService.GetPaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_TransportationTable", data.Items);
        }

        [Route("create-transportation")]
        [WebRoleFilter(false)]
        public async Task<IActionResult> Create()
        {
            var warehouses = await _warehouseService.GetWarehousesByStatus((int)EWarehouseStatus.Active);
            return View(warehouses);
        }

        [Route("find-code")]
        [WebRoleFilter(false)]
        public async Task<IActionResult> FindCode()
        {
            var warehouses = await _warehouseService.GetWarehousesByStatus((int)EWarehouseStatus.Active);
            return View(warehouses);
        }

        [Route("create-transportation-special-ship")]
        [WebRoleFilter(false)]
        public async Task<IActionResult> CreateSpecialShip()
        {
            var warehouses = await _warehouseService.GetWarehousesByStatus((int)EWarehouseStatus.Active);
            return View(warehouses);
        }

        [Route("transportation-detail/{id}", Name = "transportation-detail")]
        [WebRoleFilter(true)]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var data = await _transportationService.GetById(id);
                return View(data);
            }
            catch
            {
                var refererUrl = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(refererUrl))
                {
                    return Redirect(refererUrl);
                }

                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public async Task<ApiResponse> Create(CreateTransportationRequest request)
        {
            await _transportationService.Create(request);
            return new ApiResponse
            {
                Message = "Tạo đơn thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> Cancel(int id)
        {
            await _transportationService.Cancel(id);
            return new ApiResponse
            {
                Message = "Hủy đơn thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }
        [WebRoleFilter(true)]
        [HttpPut]
        public async Task<ApiResponse> Update([FromQuery] int id, [FromForm] UpdateTransportationRequest request)
        {
            await _transportationService.Update(id, request);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> UpdateUserUploadImage([FromForm] UpdateTransportationUserUploadImageRequest request)
        {
            await _transportationService.UpdateUserUploadImage(request);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> UpdateUserNote([FromQuery] string barcode, [FromQuery] string note)
        {
            await _transportationService.UpdateUserNote(barcode, note);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> UpdateShipId([FromQuery] string barcode, [FromQuery] int shipId)
        {
            await _transportationService.UpdateShipId(barcode, shipId);
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
            await _transportationService.DeleteSelected(ids);
            return new ApiResponse
            {
                Message = "Xóa thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, (int)ERoleId.Manager)]
        [HttpDelete]
        public async Task<ApiResponse> DeleteFilterd([FromBody] TransportationSearch search)
        {
            await _transportationService.DeleteFilterd(search);
            return new ApiResponse
            {
                Message = "Xóa thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpGet]
        public async Task<IActionResult> Export(TransportationSearch search)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Username";
                worksheet.Cell(1, 3).Value = "Mã vận đơn";
                worksheet.Cell(1, 4).Value = "Bao hàng";
                worksheet.Cell(1, 5).Value = "Cân nặng";
                worksheet.Cell(1, 6).Value = "Số khối";
                worksheet.Cell(1, 7).Value = "Đơn giá cân";
                worksheet.Cell(1, 8).Value = "Đơn giá khối";
                worksheet.Cell(1, 9).Value = "Kho TQ";
                worksheet.Cell(1, 10).Value = "Kho VN";
                worksheet.Cell(1, 11).Value = "PTVC";
                worksheet.Cell(1, 12).Value = "Cước vật tư";
                worksheet.Cell(1, 13).Value = "Tổng tiền";
                worksheet.Cell(1, 14).Value = "Ngày tạo";
                worksheet.Cell(1, 15).Value = "Ngày về kho đích";
                worksheet.Cell(1, 16).Value = "Ngày yêu cầu xuất kho";
                worksheet.Cell(1, 17).Value = "Ngày xuất kho";
                worksheet.Cell(1, 18).Value = "Trạng thái";
                worksheet.Cell(1, 19).Value = "Số kiện";

                // **Định dạng tiêu đề in đậm**
                //var headerRange = worksheet.Range("A1:F1");
                //headerRange.Style.Font.Bold = true;
                //headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                //headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                search.PageIndex = 1;
                search.PageSize = int.MaxValue;
                var data = await _transportationService.GetPaging(search);
                var result = data.Items;

                // **Ghi dữ liệu vào Excel**
                int row = 2; // Dòng bắt đầu từ 2 (sau tiêu đề)
                foreach (var item in result)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.AccountName;
                    worksheet.Cell(row, 3).Value = item.Barcode;
                    worksheet.Cell(row, 4).Value = item.BigPackageName;
                    worksheet.Cell(row, 5).Value = item.Weight;
                    worksheet.Cell(row, 6).Value = item.Volume;
                    worksheet.Cell(row, 7).Value = item.UnitWeight;
                    worksheet.Cell(row, 8).Value = item.UnitVolume;
                    worksheet.Cell(row, 9).Value = item.WarehouseFrom;
                    worksheet.Cell(row, 10).Value = item.WarehouseTo;
                    worksheet.Cell(row, 11).Value = item.ShipName;
                    worksheet.Cell(row, 12).Value = item.SurchargeVND;
                    worksheet.Cell(row, 13).Value = item.TotalPriceVND;
                    worksheet.Cell(row, 14).Value = FormatDate.FormatNullDate(item.Created);
                    worksheet.Cell(row, 15).Value = FormatDate.FormatNullDate(item.DateArrivedAtVNWarehouse);
                    worksheet.Cell(row, 16).Value = "";
                    worksheet.Cell(row, 17).Value = FormatDate.FormatNullDate(item.DateCompleted);
                    worksheet.Cell(row, 18).Value = ETransportationStatusName.GetStatusName(item.Status);
                    worksheet.Cell(row, 19).Value = item.Quantity;
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
                        "data.xlsx");
                }
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> ExportSpecialFile(TransportationSearch search)
        //{
        //    using var workbook = new XLWorkbook();

        //    search.PageIndex = 1;
        //    search.PageSize = int.MaxValue;
        //    var transportations = await _transportationService.GetPaging(search);

        //    foreach (var transportation in transportations.Items)
        //    {
        //        var fileUrl = transportation.UserUploadImage;
        //        if (string.IsNullOrEmpty(fileUrl))
        //            continue;

        //        using var httpClient = new HttpClient();
        //        var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

        //        using var sourceStream = new MemoryStream(fileBytes);
        //        using var sourceWorkbook = new XLWorkbook(sourceStream);

        //        var sheets = sourceWorkbook.Worksheets.ToList();

        //        // ===== Sheet 1
        //        var sheet1 = sheets.First();
        //        var mainSheetName = $"{transportation.AccountName}_{transportation.Barcode}";
        //        var targetSheet = workbook.Worksheets.Add(mainSheetName);

        //        CopySheetFull(sheet1, targetSheet);

        //        // ===== Sheet Tem (LUÔN TẠO)
        //        var temSheetName = $"Tem_{transportation.Barcode}";
        //        var targetTemSheet = workbook.Worksheets.Add(temSheetName);

        //        if (sheets.Count > 1)
        //        {
        //            var sheet2 = sheets[1];
        //            CopySheetFull(sheet2, targetTemSheet);
        //        }
        //    }

        //    using var stream = new MemoryStream();
        //    workbook.SaveAs(stream);
        //    stream.Position = 0;

        //    return File(
        //        stream.ToArray(),
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        "Export_Special.xlsx"
        //    );
        //}

        //private void CopySheetFull(IXLWorksheet source, IXLWorksheet target)
        //{
        //    // ===== Copy cell
        //    var range = source.RangeUsed();
        //    if (range != null)
        //    {
        //        range.CopyTo(target.Cell(1, 1));
        //    }

        //    // ===== Copy column width
        //    foreach (var col in source.ColumnsUsed())
        //    {
        //        target.Column(col.ColumnNumber()).Width = col.Width;
        //    }

        //    // ===== Copy row height
        //    foreach (var row in source.RowsUsed())
        //    {
        //        target.Row(row.RowNumber()).Height = row.Height;
        //    }

        //    // ===== Copy pictures
        //    foreach (var pic in source.Pictures)
        //    {
        //        using var ms = new MemoryStream();
        //        pic.ImageStream.CopyTo(ms);
        //        ms.Position = 0;

        //        var newPic = target.AddPicture(ms);

        //        // Vị trí
        //        newPic.MoveTo(
        //            target.Cell(
        //                pic.TopLeftCell.Address.RowNumber,
        //                pic.TopLeftCell.Address.ColumnNumber
        //            )
        //        );

        //        // Kích thước (QUAN TRỌNG)
        //        newPic.Width = pic.Width;
        //        newPic.Height = pic.Height;
        //    }

        //    // ===== AutoFit
        //    target.Columns().AdjustToContents();
        //    target.Rows().AdjustToContents();
        //}

        [HttpGet]
        public async Task<IActionResult> ExportSpecialFile(TransportationSearch search)
        {
            using var workbook = new XLWorkbook();
            using var httpClient = new HttpClient();

            search.PageIndex = 1;
            search.PageSize = int.MaxValue;

            var transportations = await _transportationService.GetPaging(search);
            var errorBarcodes = new List<string>();
            foreach (var transportation in transportations.Items)
            {
                var fileUrl = transportation.UserUploadImage;
                if (string.IsNullOrWhiteSpace(fileUrl))
                    continue;

                try
                {
                    var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

                    using var sourceStream = new MemoryStream(fileBytes);
                    using var sourceWorkbook = new XLWorkbook(sourceStream);

                    if (HasInvalidFormula(sourceWorkbook))
                    {
                        if (HasInvalidFormula(sourceWorkbook))
                        {
                            Console.WriteLine($"File lỗi formula: {fileUrl}");

                            errorBarcodes.Add(transportation.Barcode);

                            continue;
                        }
                        continue;
                    }

                    var sheets = sourceWorkbook.Worksheets.ToList();
                    if (!sheets.Any())
                        continue;

                    var baseMainName = BuildSafeSheetName(
                        $"{transportation.AccountName}_{transportation.Barcode}");

                    var mainSheetName = GetUniqueSheetName(workbook, baseMainName);

                    var baseTemName = BuildSafeSheetName(
                        $"Tem_{transportation.Barcode}");

                    var temSheetName = GetUniqueSheetName(workbook, baseTemName);

                    // ===== Copy sheet chính
                    var mainSheet = sheets.First();
                    mainSheet.CopyTo(workbook, mainSheetName);

                    // ===== Copy sheet Tem
                    if (sheets.Count > 1)
                    {
                        var temSheet = sheets[1];
                        var newTemSheet = temSheet.CopyTo(workbook, temSheetName);

                        // FIX: Sheet1 -> sheet chính
                        FixFormulaSheetReference(newTemSheet, "Sheet1", mainSheetName);
                    }
                    else
                    {
                        workbook.Worksheets.Add(temSheetName);
                    }
                }
                catch
                {
                    errorBarcodes.Add(transportation.Barcode);
                    continue;
                }
            }

            if (errorBarcodes.Any())
            {
                var errorSheetName = GetUniqueSheetName(workbook, "MVL Loi");
                var ws = workbook.Worksheets.Add(errorSheetName);

                // Header
                ws.Cell(1, 1).Value = "Mã vận đơn";

                // Data
                for (int i = 0; i < errorBarcodes.Count; i++)
                {
                    ws.Cell(i + 2, 1).Value = errorBarcodes[i];
                }

                // Format nhẹ cho dễ nhìn
                ws.Column(1).AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            stream.Flush();          // đảm bảo ghi hết dữ liệu
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Export_Special.xlsx"
            );
        }

        private string BuildSafeSheetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Sheet";

            var invalidChars = new[] { '[', ']', '*', '?', '/', '\\', ':' };

            foreach (var c in invalidChars)
                name = name.Replace(c, '_');

            // cắt ngắn để tránh vượt 31 ký tự khi thêm _1, _2
            if (name.Length > 25)
                name = name.Substring(0, 25);

            return name;
        }

        private string GetUniqueSheetName(XLWorkbook workbook, string baseName)
        {
            var name = baseName;
            int i = 1;

            while (workbook.Worksheets.Any(w => w.Name == name))
            {
                var suffix = $"_{i}";

                var maxLength = 31 - suffix.Length;
                var trimmedBase = baseName.Length > maxLength
                    ? baseName.Substring(0, maxLength)
                    : baseName;

                name = trimmedBase + suffix;
                i++;
            }

            return name;
        }

        private void FixFormulaSheetReference(
    IXLWorksheet sheet,
    string oldSheetName,
    string newSheetName)
        {
            foreach (var cell in sheet.CellsUsed(c => c.HasFormula))
            {
                var formula = cell.FormulaA1;

                if (formula.Contains(oldSheetName + "!"))
                {
                    cell.FormulaA1 = formula.Replace(
                        oldSheetName + "!",
                        newSheetName + "!"
                    );
                }
            }
        }

        private bool HasInvalidFormula(XLWorkbook workbook)
        {
            foreach (var sheet in workbook.Worksheets)
            {
                foreach (var cell in sheet.CellsUsed(c => c.HasFormula))
                {
                    var formula = cell.FormulaA1;

                    if (string.IsNullOrWhiteSpace(formula))
                        continue;

                    // check các lỗi phổ biến
                    if (formula.Contains("#REF!") ||
                        formula.Contains("#NAME?") ||
                        formula.Contains("#VALUE!") ||
                        formula.Contains("#DIV/0!"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        [HttpGet]
        public async Task<IActionResult> ExportProduct(int id)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Thiết lập tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Tên hàng";
                worksheet.Cell(1, 3).Value = "Số lượng";
                worksheet.Cell(1, 4).Value = "Kích thước";
                worksheet.Cell(1, 5).Value = "Công suất điện áp, chất liệu";
                worksheet.Cell(1, 6).Value = "File SP";

                // **Định dạng tiêu đề in đậm**
                //var headerRange = worksheet.Range("A1:F1");
                //headerRange.Style.Font.Bold = true;
                //headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                //headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // **Lấy dữ liệu thống kê**
                var data = await _transportationService.GetById(id);
                var result = data.Products;

                // **Ghi dữ liệu vào Excel**
                int row = 2; // Dòng bắt đầu từ 2 (sau tiêu đề)
                int stt = 1; // Số thứ tự
                foreach (var item in result)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = item.Name;
                    worksheet.Cell(row, 3).Value = item.Quantity;
                    worksheet.Cell(row, 4).Value = item.Dimensions;
                    worksheet.Cell(row, 5).Value = item.OtherInfor;
                    worksheet.Cell(row, 6).Value = item.Image;
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
                        "data.xlsx");
                }
            }
        }
        // [HttpGet("search-code")]
        // public async Task<IActionResult> SearchCode([FromQuery] string? name, [FromQuery] string? hscode)
        // {
        //     var result = await _transportationService.SearchCode(name, hscode);
        //     return Json(result);
        // }
    }
}
