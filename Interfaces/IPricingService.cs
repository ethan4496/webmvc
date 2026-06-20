using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IPricingService
    {
        Task<PagedList<PricingResponse>> GetPaging(PricingSearch search);
        Task<bool> Create(Pricing request);
        Task<bool> Update(int id, UpdatePricingRequest request);
        Task<bool> Delete(int id);
    }
}
