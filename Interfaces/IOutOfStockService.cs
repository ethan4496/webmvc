using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IOutOfStockService
    {
        Task<PagedList<OutOfStockResponse>> GetPaging(OutOfStockSearch search);
        Task<OutOfStockResponse> GetById(int id);
        Task<int> Create(CreateOutOfStockRequest request);
        Task<bool> Export(int id);
        Task<bool> Cancel(int id);
        Task<bool> Paied(int id);
        Task<string> RenderDeliveryNote(List<int> ids);
        Task<PagedList<ManageOutOfStockResponse>> GetPagingManage(OutOfStockSearch search);
        Task<bool> SendDeliveryNote(int id);
        Task<bool> UpdateDeliveryInfo(int id, string deliveryInfo);
        Task<bool> UpdateTotalPriceVND(int id, decimal totalPriceVND);
        Task<bool> UpdatePostOffice(int id, string postOffice);
        Task<bool> UpdateDeliveryMethod(int id, string deliveryMethod);
        Task<bool> SendRequest(List<int> ids);
        Task<string> GetDeliveryNoteUnpaidOfCurrentAccount();
        Task<bool> SendRequestForAPI(List<int> ids, Account currentAccount, DateTime currentDate);
        Task<bool> ExportForAPI(int id, Account currentAccount);
        void SendEmailDeliveryNote(int id, string customerUsername, string customerEmail, string fileHtml);
        Task<bool> SendOutStockNotis(List<int> ids);
        Task<bool> ExportList(List<int> ids);
        Task<bool> UpdateIsPrintTemp(int id);
        Task<List<TransportationResponse>> GetTransportationsInOutStock(List<int> outStockIds);
    }
}
