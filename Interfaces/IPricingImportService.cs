using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IPricingImportService
    {
        Task ImportExcelAsync(IFormFile file);
        Task<PagedResult<PriceImport>> GetListAsync(PriceImportFilter filter);
        Task UpdateAsync(PriceImport model);
        Task DeleteAsync(int id);
        // Task<PriceImport?> GetByIdAsync(int id);
    }
}
