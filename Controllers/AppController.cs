using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Responses;
using WebMVC.Services;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [Route("api")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly ITransportationService _transportationService;
        private readonly IAccountService _accountService;
        private readonly IAppApiService _appApiService;

        public AppController(ITransportationService transportationService, IAccountService accountService, IAppApiService appApiService)
        {
            _transportationService = transportationService;
            _accountService = accountService;
            _appApiService = appApiService;
        }

        [HttpGet("scan-tq")]
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

        [HttpGet("get-sale")]
        public async Task<ApiResponse> GetSale()
        {
            string token = Request.Headers["Authorization"];
            var currentAccount = AesEncryptionHelper.DecryptCurrentAccount(token);
            var sales = await _accountService.GetSales(currentAccount.Id);

            return new ApiResponse()
            {
                Data = sales,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpGet("user-chat")]
        public async Task<ApiResponse> GetUserByUsername(string username, string token)
        {
            return new ApiResponse()
            {
                Data = await _accountService.GetUserByUsernameChat(username, token),
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
            };
        }

        [HttpPost("Login")]
        public async Task<ResponseClass> Login([FromBody] SignInRequest request)
        {
            return await _appApiService.Login(request);
        }

        [HttpPost("Register")]
        public async Task<ResponseClass> Register([FromBody] SignUpRequest request)
        {
            return await _appApiService.Register(request);
        }

        [HttpPost("CheckLogin")]
        public async Task<ResponseClass> CheckLogin([FromBody] BaseAppRequest request)
        {
            return await _appApiService.CheckLogin(request);
        }

        [HttpPost("ForgotPassword")]
        public async Task<ResponseClass> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            return await _appApiService.ForgotPassword(request);
        }

        [HttpPost("LogOut")]
        public async Task<ResponseClass> LogOut([FromBody] BaseAppRequest request)
        {
            return await _appApiService.LogOut(request);
        }

        [HttpPost("UpdateDeviceToken")]
        public async Task<ResponseClass> UpdateDeviceToken([FromBody] UpdateOneSignalIdRequest request)
        {
            return await _appApiService.UpdateDeviceToken(request);
        }

        [HttpPost("ListTransportation")]
        public async Task<ResponseClass> ListTransportation([FromBody] ListTransportationRequest request)
        {
            return await _appApiService.ListTransportation(request);
        }

        [HttpPost("WarehouseShipType")]
        public async Task<ResponseClass> WarehouseShipType()
        {
            return await _appApiService.WarehouseShipType();
        }

        [HttpPost("CreateTransportation")]
        public async Task<ResponseClass> CreateTransportation([FromBody] CreateTransportationAppRequest request)
        {
            return await _appApiService.CreateTransportation(request);
        }

        [HttpPost("CreateSpecialShip")]
        public async Task<ResponseClass> CreateSpecialShip([FromBody] CreateSpecialAppRequest request)
        {
            return await _appApiService.CreateSpeicalShip(request);
        }

        [HttpPost("UpdateSpecialShipProduct")]
        public async Task<ResponseClass> UpdateSpecialShipProduct([FromBody] UpdateTransportationProductAppRequest request)
        {
            return await _appApiService.UpdateTransportationProduct(request);
        }

        [HttpPost("Tracking")]
        public async Task<ResponseClass> Tracking([FromBody] TrackingRequest request)
        {
            return await _appApiService.Tracking(request);
        }

        [HttpPost("CancelOrder")]
        public async Task<ResponseClass> CancelOrder([FromBody] HandleIdRequest request)
        {
            return await _appApiService.CancelOrder(request);
        }

        [HttpPost("AccountInfo")]
        public async Task<ResponseClass> AccountInfo([FromBody] BaseAppRequest request)
        {
            return await _appApiService.AccountInfo(request);
        }

        [HttpPost("UpdateAccountInfo")]
        public async Task<ResponseClass> UpdateAccountInfo([FromBody] UpdateAccountInfoRequest request)
        {
            return await _appApiService.UpdateAccountInfo(request);
        }

        [HttpPost("UploadFile")]
        public async Task<ResponseClass> UploadFile([FromForm] IFormFile file)
        {
            return await _appApiService.UploadFile(file);
        }

        [HttpPost("ContactConfig")]
        public async Task<ResponseClass> ContactConfig()
        {
            return await _appApiService.ContactConfig();
        }

        [HttpPost("CreateTracking")]
        public async Task<ResponseClass> CreateTracking([FromBody] CreateTrackingAppRequest request)
        {
            return await _appApiService.CreateTracking(request);
        }

        [HttpPost("TransOrderDetail")]
        public async Task<ResponseClass> TransOrderDetail([FromBody] TrackingRequest request)
        {
            return await _appApiService.TransOrderDetail(request);
        }

        [HttpPost("ListNotification")]
        public async Task<ResponseClass> ListNotification([FromBody] NotificationRequest request)
        {
            return await _appApiService.ListNotification(request, false);
        }

        [HttpPost("ListNotificationOffice")]
        public async Task<ResponseClass> ListNotificationOffice([FromBody] NotificationRequest request)
        {
            return await _appApiService.ListNotification(request, true);
        }

        [HttpPost("TotalNotification")]
        public async Task<ResponseClass> TotalNotification([FromBody] BaseAppRequest request)
        {
            return await _appApiService.TotalNotification(request, false);
        }

        [HttpPost("TotalNotificationOffice")]
        public async Task<ResponseClass> TotalNotificationOffice([FromBody] BaseAppRequest request)
        {
            return await _appApiService.TotalNotification(request, true);
        }

        [HttpPost("UpdateAllNotification")]
        public async Task<ResponseClass> UpdateAllNotification([FromBody] BaseAppRequest request)
        {
            return await _appApiService.UpdateAllNotification(request);
        }

        [HttpPost("UpdateNotification")]
        public async Task<ResponseClass> UpdateNotification([FromBody] HandleIdRequest request)
        {
            return await _appApiService.UpdateNotification(request);
        }

        [HttpPost("TotalUnReadMessage")]
        public async Task<ResponseClass> TotalUnReadMessage([FromBody] BaseAppRequest request)
        {
            return await _appApiService.TotalUnReadMessage(request);
        }

        [HttpPost("ListVoucher")]
        public async Task<ResponseClass> ListVoucher([FromBody] BaseAppRequest request)
        {
            return await _appApiService.ListVoucher(request);
        }

        [HttpPost("ListExportRequest")]
        public async Task<ResponseClass> ListExportRequest([FromBody] ExportRequestTurnRequest request)
        {
            return await _appApiService.ListExportRequest(request);
        }

        [HttpPost("GetPXKByID")]
        public async Task<ResponseClass> GetPXKByID([FromBody] HandleIdRequest request)
        {
            return await _appApiService.GetPXKByID(request);
        }

        [HttpPost("GetAllPXK")]
        public async Task<ResponseClass> GetAllPXK([FromBody] BaseAppRequest request)
        {
            return await _appApiService.GetAllPXK(request);
        }

        [HttpPost("UpdateHTGH")]
        public async Task<ResponseClass> UpdateHTGH([FromBody] HandleIdRequest request)
        {
            return await _appApiService.UpdateHTGH(request);
        }

        [HttpPost("UpdateTTNH")]
        public async Task<ResponseClass> UpdateTTNH([FromBody] HandleIdRequest request)
        {
            return await _appApiService.UpdateTTNH(request);
        }

        [HttpPost("SendRequestOutStock")]
        public async Task<ResponseClass> SendRequestOutStock([FromBody] HandleIdRequest request)
        {
            return await _appApiService.SendRequestOutStock(request);
        }

        [HttpPost("UpdateCallPhone")]
        public async Task<ResponseClass> UpdateCallPhone([FromBody] HandleIdRequest request)
        {
            return await _appApiService.UpdateCallPhone(request);
        }

        [HttpPost("OutStockAdmin")]
        public async Task<ResponseClass> OutStockAdmin([FromBody] HandleIdRequest request)
        {
            return await _appApiService.OutStockAdmin(request);
        }

        [HttpPost("SendNotiAdmin")]
        public async Task<ResponseClass> SendNotiAdmin([FromBody] HandleIdRequest request)
        {
            return await _appApiService.SendNotiAdmin(request);
        }

        [HttpPost("UpdatePostOffice")]
        public async Task<ResponseClass> UpdatePostOffice([FromBody] HandleIdRequest request)
        {
            return await _appApiService.UpdatePostOffice(request);
        }


        [HttpPost("LoginStaff")]
        public async Task<ResponseClass> LoginStaff([FromBody] SignInRequest request)
        {
            return await _appApiService.LoginStaff(request);
        }

        [HttpPost("GetPXKByIDAdmin")]
        public async Task<ResponseClass> GetPXKByIDAdmin([FromBody] HandleIdRequest request)
        {
            return await _appApiService.GetPXKByID(request);
        }

        [HttpPost("ListExportRequestAdmin")]
        public async Task<ResponseClass> ListExportRequestAdmin([FromBody] ExportRequestTurnAdminRequest request)
        {
            return await _appApiService.ListExportRequestAdmin(request);
        }

        [HttpPost("GetListAllSmallPackage")]
        public async Task<ResponseClass> GetListAllSmallPackage([FromBody] ListTransportationAppAdminRequest request)
        {
            return await _appApiService.GetListAllSmallPackage(request);
        }

        [HttpPost("UpdateBigPackageName")]
        public async Task<ResponseClass> UpdateBigPackageName([FromBody] UpdateBigPackageNameRequest request)
        {
            return await _appApiService.UpdateBigPackageName(request);
        }

        [HttpPost("RollBackSmallPackage")]
        public async Task<ResponseClass> RollBackSmallPackage([FromBody] HandleIdRequest request)
        {
            return await _appApiService.RollBackSmallPackage(request);
        }

        [HttpPost("GetDataToCreateBigPackage")]
        public async Task<ResponseClass> GetDataToCreateBigPackage([FromBody] GetDataToCreateBigPackageRequest request)
        {
            return await _appApiService.GetDataToCreateBigPackage(request);
        }

        [HttpPost("GetListBigPackage")]
        public async Task<ResponseClass> GetListBigPackage([FromBody] ListBigPackageRequest request)
        {
            return await _appApiService.GetListBigPackage(request);
        }

        [HttpPost("UpdateBigPackage")]
        public async Task<ResponseClass> UpdateBigPackage([FromBody] UpdateBigPackageAppRequest request)
        {
            return await _appApiService.UpdateBigPackage(request);
        }

        [HttpPost("AddSmallPackageToBigPackage")]
        public async Task<ResponseClass> AddSmallPackageToBigPackage([FromBody] AddSmallPackageToBigPackageRequest request)
        {
            return await _appApiService.AddSmallPackageToBigPackage(request);
        }

        [HttpPost("GetBigPackageByID")]
        public async Task<ResponseClass> GetBigPackageByID([FromBody] HandleIdRequest request)
        {
            return await _appApiService.GetBigPackageByID(request);
        }

        [HttpPost("CreateBigPackage")]
        public async Task<ResponseClass> CreateBigPackage([FromBody] CreateBigPackageAppRequest request)
        {
            return await _appApiService.CreateBigPackage(request);
        }

        [HttpPost("GetListSmallPackage")]
        public async Task<ResponseClass> GetListSmallPackage([FromBody] ListSmallPackageRequest request)
        {
            return await _appApiService.GetListSmallPackage(request);
        }

        [HttpPost("UpdateSmallPackages")]
        public async Task<ResponseClass> UpdateSmallPackages([FromBody] UpdateSmallPackageAppRequest request)
        {
            return await _appApiService.UpdateSmallPackages(request);
        }
        [HttpPost("CreateSmallPackage")]
        public async Task<ResponseClass> CreateSmallPackage([FromBody] CreateSmallPackageAppRequest request)
        {
            return await _appApiService.CreateSmallPackage(request);
        }

        [HttpPost("UpdateSmallPackagesInVN")]
        public async Task<ResponseClass> UpdateSmallPackagesInVN([FromBody] UpdateSmallPackageAppRequest request)
        {
            return await _appApiService.UpdateSmallPackagesInVN(request);
        }

        [HttpPost("GetSmallPackage")]
        public async Task<ResponseClass> GetSmallPackage([FromBody] TrackingRequest request)
        {
            return await _appApiService.GetSmallPackage(request);
        }

        [HttpPost("DeleteSmallPackage")]
        public async Task<ResponseClass> DeleteSmallPackage([FromBody] TrackingRequest request)
        {
            return await _appApiService.DeleteSmallPackage(request);
        }

        [HttpPost("UpdateUserNote")]
        public async Task<ResponseClass> UpdateUserNote([FromBody] UpdateUserNoteRequest request)
        {
            return await _appApiService.UpdateUserNote(request);
        }

        [HttpPost("UpdateShipId")]
        public async Task<ResponseClass> UpdateShipId([FromBody] UpdateShipIdRequest request)
        {
            return await _appApiService.UpdateShipId(request);
        }

        [HttpPost("UpdateUserUploadImage")]
        public async Task<ResponseClass> UpdateUserUploadImage([FromBody] UpdateUserNoteRequest request)
        {
            return await _appApiService.UpdateUserUploadImage(request);
        }
    }
}
