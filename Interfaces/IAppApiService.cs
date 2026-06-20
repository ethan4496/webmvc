using WebMVC.Services;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IAppApiService
    {
        Task<ResponseClass> AccountInfo(BaseAppRequest request);
        Task<ResponseClass> AddSmallPackageToBigPackage(AddSmallPackageToBigPackageRequest request);
        Task<ResponseClass> CancelOrder(HandleIdRequest request);
        Task<ResponseClass> CheckLogin(BaseAppRequest request);
        Task<ResponseClass> ContactConfig();
        Task<ResponseClass> CreateBigPackage(CreateBigPackageAppRequest request);
        Task<ResponseClass> CreateSmallPackage(CreateSmallPackageAppRequest request);
        Task<ResponseClass> CreateSpeicalShip(CreateSpecialAppRequest request);
        Task<ResponseClass> CreateTracking(CreateTrackingAppRequest request);
        Task<ResponseClass> CreateTransportation(CreateTransportationAppRequest request);
        Task<ResponseClass> DeleteSmallPackage(TrackingRequest request);
        Task<ResponseClass> ForgotPassword(ForgotPasswordRequest request);
        Task<ResponseClass> GetAllPXK(BaseAppRequest request);
        Task<ResponseClass> GetBigPackageByID(HandleIdRequest request);
        Task<ResponseClass> GetDataToCreateBigPackage(GetDataToCreateBigPackageRequest request);
        Task<ResponseClass> GetListAllSmallPackage(ListTransportationAppAdminRequest request);
        Task<ResponseClass> GetListBigPackage(ListBigPackageRequest request);
        Task<ResponseClass> GetListSmallPackage(ListSmallPackageRequest request);
        Task<ResponseClass> GetPXKByID(HandleIdRequest request);
        Task<ResponseClass> GetSmallPackage(TrackingRequest request);
        Task<ResponseClass> ListExportRequest(ExportRequestTurnRequest request);
        Task<ResponseClass> ListExportRequestAdmin(ExportRequestTurnAdminRequest request);
        Task<ResponseClass> ListNotification(NotificationRequest request, bool isStaff);
        Task<ResponseClass> ListTransportation(ListTransportationRequest request);
        Task<ResponseClass> ListVoucher(BaseAppRequest request);
        Task<ResponseClass> Login(SignInRequest request);
        Task<ResponseClass> LoginStaff(SignInRequest request);
        Task<ResponseClass> LogOut(BaseAppRequest request);
        Task<ResponseClass> OutStockAdmin(HandleIdRequest request);
        Task<ResponseClass> Register(SignUpRequest request);
        Task<ResponseClass> RollBackSmallPackage(HandleIdRequest request);
        Task<ResponseClass> SendNotiAdmin(HandleIdRequest request);
        Task<ResponseClass> SendRequestOutStock(HandleIdRequest request);
        Task<ResponseClass> TotalNotification(BaseAppRequest request, bool isStaff);
        Task<ResponseClass> TotalUnReadMessage(BaseAppRequest request);
        Task<ResponseClass> Tracking(TrackingRequest request);
        Task<ResponseClass> TransOrderDetail(TrackingRequest request);
        Task<ResponseClass> UpdateAccountInfo(UpdateAccountInfoRequest request);
        Task<ResponseClass> UpdateAllNotification(BaseAppRequest request);
        Task<ResponseClass> UpdateBigPackage(UpdateBigPackageAppRequest request);
        Task<ResponseClass> UpdateBigPackageName(UpdateBigPackageNameRequest request);
        Task<ResponseClass> UpdateCallPhone(HandleIdRequest request);
        Task<ResponseClass> UpdateDeviceToken(UpdateOneSignalIdRequest request);
        Task<ResponseClass> UpdateHTGH(HandleIdRequest request);
        Task<ResponseClass> UpdateNotification(HandleIdRequest request);
        Task<ResponseClass> UpdatePostOffice(HandleIdRequest request);
        Task<ResponseClass> UpdateShipId(UpdateShipIdRequest request);
        Task<ResponseClass> UpdateSmallPackages(UpdateSmallPackageAppRequest request);
        Task<ResponseClass> UpdateSmallPackagesInVN(UpdateSmallPackageAppRequest request);
        Task<ResponseClass> UpdateTransportationProduct(UpdateTransportationProductAppRequest request);
        Task<ResponseClass> UpdateTTNH(HandleIdRequest request);
        Task<ResponseClass> UpdateUserNote(UpdateUserNoteRequest request);
        Task<ResponseClass> UpdateUserUploadImage(UpdateUserNoteRequest request);
        Task<ResponseClass> UploadFile(IFormFile request);
        Task<ResponseClass> WarehouseShipType();
    }
}
