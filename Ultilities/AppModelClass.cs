using Newtonsoft.Json;
using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Ultilities
{
    public class UpdateShipIdRequest : BaseAppRequest
    {
        public string Barcode { get; set; }
        public int ShipId { get; set; }

    }
    public class UpdateUserNoteRequest : BaseAppRequest
    {
        public string Barcode { get; set; }
        public string Note { get; set; }

    }
    public class UpdateSmallPackageAppRequest : BaseAppRequest
    {
        public int Type { get; set; }
        public List<SmallPackage> SmallPackages { get; set; }
    }
    public class SmallPackage
    {
        public int ID { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public double? AdditionFeeCYN { get; set; }
        public string IMGPackage { get; set; }
        public string OrderTransactionCode { get; set; }
        public string ShippingTypeName { get; set; }
        public int Status { get; set; }
        public int? BigPackageId { get; set; }
        public string ProductQuantity { get; set; }
        public string Username { get; set; }
        public int? ShippingType { get; set; }
    }
    public class CreateSmallPackageAppRequest : BaseAppRequest
    {
        public string Barcode { get; set; }
        public string Username { get; set; }
        public int Type { get; set; }
    }
    public class ListSmallPackageRequest : BaseAppRequest
    {
        public int PageIndex { get; set; }
        public int Type { get; set; }
        public string Code { get; set; }
        public string FD { get; set; }
        public string TD { get; set; }
    }
    public class CreateBigPackageAppRequest : BaseAppRequest
    {
        public string Code { get; set; }
        public string PartnerInfo { get; set; }
        public double TotalWeight { get; set; }
        public double TotalVolume { get; set; }
        public int TotalPackage { get; set; }
        public List<int> SmallPackageIDs { get; set; }
    }
    public class BigPackageDetailApp
    {
        public int ID { get; set; }
        public string PackageCode { get; set; }
        public int Status { get; set; }
        public double Volume { get; set; }
        public double Weight { get; set; }
        public int TotalPackage { get; set; }
        public string PartnerInfor { get; set; }
        public List<SmallPachageBigPackageDetail> SmallPackages { get; set; } = new List<SmallPachageBigPackageDetail>();
    }

    public class SmallPachageBigPackageDetail
    {
        public int ID { get; set; }
        public string OrderTransactionCode { get; set; }
        public string ProductQuantity { get; set; }
        public double Volume { get; set; }
        public double Weight { get; set; }
        public DateTime? DateInTQWarehouse { get; set; }
    }
    public class AddSmallPackageToBigPackageRequest : BaseAppRequest
    {
        public string Barcode { get; set; }
        public int BigPackageID { get; set; }
    }
    public class BigPackagePackageOfListApp
    {
        public int ID { get; set; }
        public string PackageCode { get; set; }
        public string Weight { get; set; }
        public string Volume { get; set; }
        public int Status { get; set; }
        public string StatusString { get; set; }
        public string CreatedDate { get; set; }
        public string TotalPackage { get; set; }
        public string PartnerInfor { get; set; }
        public int TotalDay { get; set; }
    }
    public class ListBigPackageRequest : BaseAppRequest
    {
        public int PageIndex { get; set; }
        public int Status { get; set; }
        public string Code { get; set; }
        public string FD { get; set; }
        public string TD { get; set; }
    }

    public class DataToCreateBigPackage
    {
        public int TotalPackage { get; set; }
        public double TotalWeight { get; set; }
        public double TotalVolume { get; set; }
        public List<int> SmallPackageIds { get; set; }
    }
    public class GetDataToCreateBigPackageRequest : BaseAppRequest
    {
        public string Code { get; set; }
        public string FD { get; set; }
        public string TD { get; set; }
        public int Type { get; set; }
    }
    public class UpdateTransportationProductAppRequest : BaseAppRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dimensions { get; set; }
        public int Quantity { get; set; }
        public string OtherInfor { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile Image { get; set; }
    }
    public class UpdateBigPackageAppRequest : BaseAppRequest
    {
        public int ID { get; set; }
        public double TotalWeight { get; set; }
        public double TotalVolume { get; set; }
        public int Status { get; set; }
        public string PartnerInfor { get; set; }
    }
    public class UpdateBigPackageNameRequest : BaseAppRequest
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
    public class ListTransportationAppAdminRequest : BaseAppRequest
    {
        public int PageIndex { get; set; }
        public int ID { get; set; }
        public int Status { get; set; }
        public string Code { get; set; }
        public string FD { get; set; }
        public string TD { get; set; }
    }
    public class SmallPackageOfListApp
    {
        public int ID { get; set; }
        public string OrderTransactionCode { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? DateInTQWarehouse { get; set; }
        public string AccountInTQWarehouse { get; set; }
        public DateTime? DateComingVNWarehouse { get; set; }
        public DateTime? DateCheck { get; set; }
        public DateTime? DateInLasteWareHouse { get; set; }
        public DateTime? DateExport { get; set; }
        public string BigPackageName { get; set; }
        public string PartnerInfor { get; set; }
        public double AdditionFeeCYN { get; set; }
        public double AdditionFeeVND { get; set; }
        public string ProductQuantity { get; set; }
        public double Volume { get; set; }
        public double Weight { get; set; }

        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case (int)ETransportationStatus.Cancel:
                        return "取消 (Đã hủy)";
                    case (int)ETransportationStatus.New:
                        return "新订单 (Đơn hàng mới)";
                    case (int)ETransportationStatus.ArrivedAtTQWarehouse:
                        return "已入库 (Nhập kho TQ)";
                    case (int)ETransportationStatus.ExitedFromTQWarehouse:
                        return "已装车 (Đã phát hàng)";
                    case (int)ETransportationStatus.CustomsInspectedGoods:
                        return "海关检查 (Kiểm hóa)";
                    case (int)ETransportationStatus.ReturningToVNWarehouse:
                        return "加工越南仓库货物 (Đang Nhập Khẩu)";
                    case (int)ETransportationStatus.ArrivedAtVNWarehouse:
                        return "河内仓库 (Nhập kho HN)";
                    case (int)ETransportationStatus.Completed:
                        return "已送货 (Đã xuất kho)";
                    default:
                        return "Không xác định";
                }
            }
        }
        public string IMGPackage { get; set; }
        public string UserUploadImage { get; set; }
        public string UserNote { get; set; }
    }

    public class ExportRequestTurnAdminRequest : BaseAppRequest
    {
        public int PageIndex { get; set; }
        public int ID { get; set; }
        public int Status { get; set; }
        public string Username { get; set; }
        public string FD { get; set; }
        public string TD { get; set; }
        public string PostOffice { get; set; }
    }
    public class UpdateSmallPackageRequest : BaseAppRequest
    {
        public List<SmallPackageRequest> SmallPackages { get; set; }
    }

    public class SmallPackageRequest
    {
        public int ID { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public double? AdditionFeeCYN { get; set; }
        public string IMGPackage { get; set; }
        public string Description { get; set; }
        public string Username { get; set; }
        public int ShippingType { get; set; }
    }
    public class ExportRequestTurnOfListResponse
    {
        public int? ID { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal? TotalPriceVND { get; set; }
        public int? Status { get; set; }
        public string PostOffice { get; set; }
        public int? StatusExport { get; set; }
        public string Note { get; set; }
        public string StaffNote { get; set; }
        public bool? IsRequest { get; set; }
        public DateTime? DateCallPhone { get; set; }

    }

    public class ExportRequestTurnRequest : BaseAppRequest
    {
        public int PageIndex { get; set; }
        public int ID { get; set; }
    }
    public class VoucherAppResponse
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Image { get; set; }
        public string EndDate { get; set; }
        public string Description { get; set; }
        public string DecreaseAmount { get; set; }
        public int Expiration { get; set; }
    }

    public class NotificationResponse
    {
        public int? ID { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Title { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
    }

    public class NotificationRequest : BaseAppRequest
    {
        public int PageIndex { get; set; }
        public int Type { get; set; }
    }
    public class CreateTrackingAppRequest : BaseAppRequest
    {
        public string Barcode { get; set; }
        public int? Warehouse { get; set; }
        public string ProductName { get; set; }
        public double? TotalWeight { get; set; }
        public double? TotalVolume { get; set; }
        public int? Number { get; set; }
        public decimal? TotalShip { get; set; }
        public decimal? ShipInVn { get; set; }
        public string Note { get; set; }
    }
    public class TransportFeeResponse
    {
        public int ShipId { get; set; }
        public string ProductName { get; set; }

        public double TotalShip { get; set; }

        public double Weight { get; set; }

        public double Volume { get; set; }

        public double ShipVn { get; set; }

        public double ShipVnVolume { get; set; }

        public int Number { get; set; }

        public double ShipInVn { get; set; }

        public double UnitPrice { get; set; }

        public double TotalPrice { get; set; }

        public string Note { get; set; }
        public string UserUploadImage { get; set; }

        public List<TransportationProduct> TransportationProducts { get; set; }
    }


    public class ContactConfigResponse
    {
        public string ZaloLink { get; set; }
        public string HotLine { get; set; }
        public string InsurancePercent { get; set; }
        public string News1 { get; set; }
    }

    public class UpdateAccountInfoRequest : BaseAppRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public string Password { get; set; }
    }


    public class ProfileResponse
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Gender { get; set; }
        public string Dob { get; set; }
        public int ToWarehouseID { get; set; }
        public int FromWarehouseID { get; set; }
        public int ShipType { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Wallet { get; set; }
    }

    public class HandleIdRequest : BaseAppRequest
    {
        public int ID { get; set; }
        public string Reason { get; set; }
    }


    public class TrackingAppResponse
    {
        public int? ID { get; set; }
        public string Barcode { get; set; }
        public int? Status { get; set; }
        public string DateTQ { get; set; }
        public string DateComingVN { get; set; }
        public string DateCheck { get; set; }
        public string DateVN { get; set; }
        public string DateProcessVN { get; set; }
        public string DateExport { get; set; }
    }

    public class TrackingRequest : BaseAppRequest
    {
        public string Barcode { get; set; }
    }
    public class CreateSpecialAppRequest : BaseAppRequest
    {
        public int? FromWarehouse { get; set; }
        public int? VnWarehouse { get; set; }
        public List<CreateTransportationItem> Items { get; set; }
    }
    public class CreateTransportationAppRequest : BaseAppRequest
    {
        public int? FromWarehouse { get; set; }
        public int? VnWarehouse { get; set; }
        public int? ShipType { get; set; }
        public List<TransportationRequests> TransportationRequests { get; set; }
    }

    public class TransportationRequests
    {
        public string Barcode { get; set; }
        public string Note { get; set; }
        public int? VoucherID { get; set; }
        public string ImageUrl { get; set; }
    }

    public class WarehouseShipTypeResponse
    {
        public List<WarehouseResponse> VNWarehouses { get; set; }
        public List<WarehouseResponse> FromWarehouses { get; set; }
        public List<WarehouseResponse> ShipTypes { get; set; }
    }
    public class WarehouseResponse
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
    public class TransportationAppResponse
    {
        public int ID { get; set; }
        public string Barcode { get; set; }
        public string StatusName { get; set; }
        public int Status { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public int Quantity { get; set; }
        public string VNWarehouse { get; set; }
        public string FromWarehouse { get; set; }
        public string ShippingType { get; set; }
        public string Note { get; set; }
        public string SensorFeeeVND { get; set; }
        public string CreatedDate { get; set; }
        public string DateExport { get; set; }
        public string ExportRequestNote { get; set; }
        public string Warning { get; set; }
    }
    public class ListTransportationRequest : BaseAppRequest
    {
        public int PageIndex { get; set; }
        public string Code { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string FD { get; set; }
        public string TD { get; set; }
    }
    public class UpdateOneSignalIdRequest : BaseAppRequest
    {
        public string Device { get; set; }
    }
    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }
    public class SignUpRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class SignInRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Type { get; set; }
        public string DeviceToken { get; set; }
        public string TypeName { get; set; }
    }

    public class BaseAppRequest
    {
        public int UID { get; set; }
        public string Key { get; set; }
    }
    public class AppAccountResponse
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string SpecialShipId { get; set; }
        public int Role { get; set; }
        public int PostOffice { get; set; }
        public int? WarehouseTypeTQ { get; set; }
    }

    public class ResponseClass
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Logout { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AppAccountResponse? Account { get; set; }
        public int? TotalPage { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalItem { get; set; }
    }

    public class APIUtils
    {
        public enum ResponseCode
        {
            SUCCESS = 102,//Success
            FAILED = 103,
            NotFound = 101,//The URI requested is invalid or the resource requested, such as a user, does not exists
            DataDupliacation = 100,//Let you know if provided data is already there.
            InternalServerError = 500,//Something is broken,

        }
        public enum ResponseMessage
        {
            Success, Fail, Error
        }
        public static string GetResponseCode(ResponseCode code)
        {
            var rs = string.Empty;
            switch (code.ToString())
            {
                case "SUCCESS":
                    rs = "102";
                    break;
                case "FAILED":
                    rs = "103";
                    break;
                case "NotFound":
                    rs = "101";
                    break;
                case "DataDupliacation":
                    rs = "100";
                    break;
            }
            return rs;
        }

    }
}
