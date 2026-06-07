using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [Authorize]
    [WebRoleFilter(true, (int)ERoleId.Sale)]
    public class ScanController : Controller
    {
        private readonly IBigPackageService _bigPackageService;
        private readonly ITransportationService _transportationService;

        public ScanController(IBigPackageService bigPackageService, ITransportationService transportationService)
        {
            _bigPackageService = bigPackageService;
            _transportationService = transportationService;
        }

        [Route("receive-warehouse/{type}", Name = "receive-warehouse")]
        public IActionResult ReceiveWarehouse(int type)
        {
            return View(type);
        }

        [Route("destination-warehouse", Name = "destination-warehouse")]
        public IActionResult DestinationWarehouse()
        {
            return View();
        }

        [Route("import-excel/{status}", Name = "import-excel")]
        public IActionResult ImportExcel(int status)
        {
            return View(status);
        }

        [HttpGet]
        public async Task<ApiResponse> ScanAtReceiveWarehouse(string barcode, int type)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            var data = await _transportationService.ScanByBarcode(barcode, type);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }
        [HttpGet]
        public async Task<ApiResponse> GetBarcodeInTQ(int type)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            var data = await _transportationService.GetBarcodeInTQ(type);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpGet]
        public async Task<ApiResponse> ScanAtDestinationWarehouse(string barcode, int? bigPackageId)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            var data = await _transportationService.ScanByBarcodeAtDestinationWarehouse(barcode, bigPackageId);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpGet]
        public async Task<ApiResponse> GetTransporationByBigPackageId(int bigPackageId)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            var data = await _transportationService.GetByBigPackageId(bigPackageId);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPost]
        public async Task<ApiResponse> CreateFloatingTransportation(CreateFloatingTransportationRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _transportationService.CreateFloatingTransportation(request.Barcode, request.Type, bigPackageId: request.BigPackageId);
            return new ApiResponse
            {
                Message = "Tạo đơn thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> UpdateBarcodeAtReceiveWarehouse([FromBody] UpdateBarcodeRequest request, int type)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            request.Status = (int)ETransportationStatus.ArrivedAtTQWarehouse;
            var data = await _transportationService.UpdateBarcode(request, type);
            return new ApiResponse
            {
                Message = "Câp nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
                Data = data
            };
        }

        [HttpPut]
        public async Task<ApiResponse> UpdateBarcodeAtDestinationWarehouse([FromBody] UpdateBarcodeRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            request.Status = (int)ETransportationStatus.ArrivedAtVNWarehouse;
            var data = await _transportationService.UpdateBarcode(request);
            return new ApiResponse
            {
                Message = "Câp nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
                Data = data
            };
        }

        [HttpPut]
        public async Task<ApiResponse> UpdateBarcodeMultipleAtReceiveWarehouse([FromBody] List<UpdateBarcodeRequest> request, int type)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _transportationService.UpdateBarcodeMultiple(request, (int)ETransportationStatus.ArrivedAtTQWarehouse, type);
            return new ApiResponse
            {
                Message = "Câp nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> AssignTransportationToCustomer([FromBody] AssignTransportationRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _transportationService.AssignTransportation(request);
            return new ApiResponse
            {
                Message = "Gán đơn thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPost]
        public async Task<ApiResponse> CreateBigPackage([FromBody] CreateBigPackageRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _bigPackageService.CreateBigPackage(request);
            return new ApiResponse
            {
                Message = "Tạo bao thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpGet]
        public IActionResult ExportSampleExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Mẫu");
                worksheet.Cell(1, 1).Value = "Mã vận đơn"; // Thêm tiêu đề cột
                worksheet.Cell(1, 2).Value = "Cân nặng"; // Thêm tiêu đề cột
                worksheet.Cell(1, 3).Value = "Số khối"; // Thêm tiêu đề cột
                worksheet.Cell(1, 4).Value = "Số kiện"; // Thêm tiêu đề cột
                worksheet.Cell(1, 5).Value = "Phụ phí (tệ)"; // Thêm tiêu đề cột
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
        public async Task<IActionResult> ImportExcel([FromQuery] int id, [FromQuery] int type, [FromForm] IFormFile file)
        {
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
                    worksheet.Cell(1, resultColumn).Value = $"Kết quả Import {ETransportationStatusName.GetStatusName(id)}";
                    worksheet.Cell(1, resultColumn).Style.Font.Bold = true;

                    int rowCount = worksheet.LastRowUsed().RowNumber();
                    for (int row = 2; row <= rowCount; row++)
                    {
                        bool success = true;
                        string failedMessage = "";
                        try
                        {
                            string trackingCode = worksheet.Cell(row, 1).GetString().Trim();
                            await _transportationService.CreateFloatingTransportation(trackingCode, type, false);
                            string quantity = worksheet.Cell(row, 2).GetString().Trim();
                            string weight = worksheet.Cell(row, 3).GetString().Trim();
                            string volume = worksheet.Cell(row, 4).GetString().Trim();
                            string surcharge = worksheet.Cell(row, 5).GetString().Trim();
                            var importData = new UpdateBarcodeRequest
                            {
                                Barcode = trackingCode,
                                Quantity = int.Parse(quantity != "" ? quantity : "1"),
                                Weight = double.Parse(weight != "" ? weight : "0"),
                                Volume = double.Parse(volume != "" ? volume : "0"),
                                Surcharge = decimal.Parse(surcharge != "" ? surcharge : "0"),
                                Status = id,
                                Type = type,
                            };

                            await _transportationService.UpdateBarcode(importData);
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
    }
}
