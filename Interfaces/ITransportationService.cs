using WebMVC.Entities;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface ITransportationService
    {
        Task<bool> AssignTransportation(AssignTransportationRequest request);
        Task<bool> Cancel(int id);
        Task<bool> Create(CreateTransportationRequest request);
        Task<bool> CreateFloatingTransportation(string barcode, int type, bool isCheckExist = false, int? bigPackageId = null);
        Task<List<TransportationResponse>> GetByBigPackageId(int bigPackageId);
        Task<PagedList<TransportationResponse>> GetPaging(TransportationSearch transportationSearch);
        Task<TransportationResponse> UpdateBarcode(UpdateBarcodeRequest request, int? type = null);
        Task<bool> UpdateBarcodeMultiple(List<UpdateBarcodeRequest> requests, int status, int type);
        Task<ScanResponse> ScanByBarcode(string barcode, int? type);
        Task<ScanResponse> ScanByBarcodeAtDestinationWarehouse(string barcode, int? bigPackageId);
        Task<List<TransportationResponse>> GetAtOutOfStock(int accountId);
        Task<TransportationResponse> GetById(int id);
        Task<bool> Update(int id, UpdateTransportationRequest request);
        Task<Transportation> ChangeStatusDateAsync(Transportation transportation, string currentAccountName, DateTime currentDate, int currentAccountId);
        Task<bool> UpdateAtOutOfStockManage(int outOfStockId, UpdateTransportationAtOutOfStockManageRequest request);
        Task<Transportation> GetByBarcode(string barcode);
        Task<bool> DeleteSelected(List<int> ids);
        Task<bool> DeleteFilterd(TransportationSearch search);
        Task<PagedList<TransportationResponse>> GetPagingForAPI(TransportationSearch search, LoggedModel loggedModel);
        Task<bool> CreateForApi(CreateTransportationRequest request, Account currentAccount);
        Task<bool> CancelForAPI(int id, Account currentAccount);
        Task<BigPackage> CalculateBigPackageInfor(int id);
        Task<bool> CreateFloatingTransportationForApi(string barcode, int type, DateTime currentDate, Account currentAccount, int? bigPackageId = null);
        Task<bool> UpdateBarcodeMultipleForApi(List<UpdateBarcodeRequest> requests, int status, int type, DateTime currentDate, Account currentAccount);
        Task<List<ScanResponse>> GetBarcodeInTQ(int type);
        Task<bool> UpdateUserUploadImage(UpdateTransportationUserUploadImageRequest request);
        Task<bool> UpdateUserNote(string barcode, string note);
        Task<bool> UpdateShipId(string barcode, int shipId);
        Task<bool> UpdateShipIdForApi(string barcode, int shipId, Account currentAccount);
        Task<bool> UpdateUserNoteForApi(string barcode, string note, Account currentAccount);
        // Task<object> SearchCode(string? name, string? hscode);
    }
}
