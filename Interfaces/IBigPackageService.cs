using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IBigPackageService
    {
        Task<bool> CreateBigPackage(CreateBigPackageRequest request);
        Task<bool> CreateBigPackageForApi(CreateBigPackageRequest request, DateTime currentDate, Account currentAccount);
        Task<bool> CreateExpense(CreateExpenseRequest request);
        Task<bool> UpdateExpense(int id, UpdateExpenseRequest request);
        Task<bool> DeleteExpense(int id);
        Task<bool> DeleteFilterd(BigPackageSearch search);
        Task<bool> DeleteSelected(List<int> ids);
        Task<BigPackageResponse> GetById(int id);
        // Task<PagedList<OutOfStockResponse>> GetOutStockByPackage(int id);
        Task<PagedList<BigPackageResponse>> GetPaging(BigPackageSearch search);
        Task<PagedList<BigPackageResponse>> GetPagingReport(BigPackageSearch search);
        Task<bool> Update(int id, UpdateBigPackageRequest request);
        Task<bool> UpdateForApi(int id, UpdateBigPackageRequest request, DateTime currentDate, Account currentAccount);
    }
}
