using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ClosedXML.Excel;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace WebMVC.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        int pageSize = 20;
        private readonly IAccountService _accountService;
        private readonly IWarehouseService _warehouseService;

        private readonly IUnitOfWork _unitOfWork;

        public AccountController(IAccountService accountService, IWarehouseService warehouseService, IUnitOfWork unitOfWork)
        {
            _accountService = accountService;
            _warehouseService = warehouseService;
            _unitOfWork = unitOfWork;
        }

        [Route("home")]
        public async Task<IActionResult> Home()
        {
            var data = await _accountService.GetDashboard();
            return View(data);
        }

        [Route("profile")]
        public async Task<IActionResult> Profile()
        {
            var model = await _accountService.GetAccountProfile();
            return View(model);
        }

        [WebRoleFilter(true, (int)ERoleId.Sale, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff)]
        [Route("staff")]
        public IActionResult Index()
        {
            return View();
        }

        [WebRoleFilter(true, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff)]
        [Route("customer")]
        public IActionResult Customer()
        {
            return View();
        }

        [WebRoleFilter(true, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff)]
        [Route("account-detail/{id}", Name = "account-detail")]
        public async Task<IActionResult> Detail(int id)
        {
            var account = await _accountService.GetById(id);
            var sales = await _accountService.GetIdAndUsernameByRole((int)ERoleId.Sale);
            var shipping = await _warehouseService.GetWarehousesByType((int)EWarehouseType.Shipping);
            var vnWarehouseStaffs = await _accountService.GetIdAndUsernameByRole((int)ERoleId.VNWarehouseStaff);
            var accountWarehouseSupervisors = await _accountService.GetAccountWarehouseSupervisorsBySaleId(id);
            return View(new
            {
                Account = account,
                ListSale = sales,
                Shipping = shipping,
                ListVNWarehouseStaff = vnWarehouseStaffs,
                SelectedListVNWarehouseStaffId = accountWarehouseSupervisors.Select(x => x.WarehouseStaffId).ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffPaging(AccountSearch search)
        {
            search.PageSize = pageSize;
            search.IsCustomer = 0;
            var data = await _accountService.GetPaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_AccountTable", data.Items);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerPaging(AccountSearch search)
        {
            search.PageSize = pageSize;
            search.RoleId = (int)ERoleId.User;
            search.IsCustomer = 1;
            var data = await _accountService.GetPaging(search);
            ViewBag.CurrentPage = search.PageIndex;
            ViewBag.PageSize = search.PageSize;
            ViewBag.TotalPages = data.TotalPage;
            return PartialView("_CustomerTable", data.Items);
        }

        [WebRoleFilter(true, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff)]
        [HttpPost]
        public async Task<ApiResponse> Create(CreateAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _accountService.Create(request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Tạo thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [WebRoleFilter(true, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff)]
        [HttpPut]
        public async Task<ApiResponse> Update([FromQuery] int id, [FromBody] UpdateAccountRequest request)
        {
            await _accountService.Update(id, request);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpPut]
        public async Task<ApiResponse> Profile(AccountProfile request)
        {
            await _accountService.UpdateProfile(request);
            return new ApiResponse
            {
                Message = "Cập nhật thành công",
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetPricingSeparate(int id)
        {
            var data = await _accountService.GetPricingSeparate(new List<int> { id });
            return PartialView("_PricingSeparateTable", data);
        }

        [HttpPost]
        public async Task<ApiResponse> CreatePricingSeparate([FromQuery] int id, CreatePricingSeparateRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _accountService.CreatePricingSeparate(id, request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Tạo thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPut]
        public async Task<ApiResponse> UpdatePricingSeparate([FromQuery] int id, PricingSeparate request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _accountService.UpdatePricingSeparate(id, request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Cập nhật thành công",
                Type = (int)EApiResponseType.Success,
            };
        }


        [HttpDelete]
        public async Task<ApiResponse> DeletePricingSeparate([FromQuery] int accountId, [FromQuery] int id)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _accountService.DeletePricingSeparate(accountId, id);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Xóa thành công",
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpGet]
        public async Task<IActionResult> Export(AccountSearch search)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Username";
                worksheet.Cell(1, 3).Value = "NV sale";
                worksheet.Cell(1, 4).Value = "Họ va tên";
                worksheet.Cell(1, 5).Value = "SDT";
                var accounts = await _accountService.getListAccount(search);
                // Console.WriteLine(
                //     "accounts"
                // );
                // Console.WriteLine(
                //     JsonSerializer.Serialize(
                //         search,
                //         new JsonSerializerOptions
                //         {
                //             WriteIndented = true
                //         }
                //     )
                // );
                // Console.WriteLine(
                //     JsonSerializer.Serialize(
                //         accounts,
                //         new JsonSerializerOptions
                //         {
                //             WriteIndented = true
                //         }
                //     )
                // );
                int row = 2;
                foreach (var item in accounts)
                {
                    
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.Username;
                    worksheet.Cell(row, 3).Value = item.SaleName;
                    worksheet.Cell(row, 4).Value = item.FullName;
                    worksheet.Cell(row, 5).Value = item.Phone;
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
                        "customers.xlsx");
                }
            }
        }

        #region Json data
        [HttpGet]
        public async Task<IActionResult> Paging(AccountSearch search)
        {
            var data = await _accountService.GetPaging(search);
            return Json(data);
        }
        #endregion
    }
}
