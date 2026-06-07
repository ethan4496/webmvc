using System.Threading.Tasks;
using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IVoucherService
    {
        Task<PagedList<VoucherResponse>> GetPaging(VoucherSearch voucherSearch);
        Task<bool> Create(CreateVoucherRequest request);
        Task<bool> Update(int id, UpdateVoucherRequest request);
        Task<List<VoucherAccount>> GetVoucherAccountByCurrentAccountId();
        Task<PagedList<VoucherAccountResponse>> GetVoucherAccountPaging(VoucherAccountSearch search);
        Task<VoucherAccountDetail> GetVoucherAccountDetail(int id);
        Task<bool> RecallAccountVoucher(int id);
    }
}
