using System.Security.Claims;
using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IAccountService
    {
        Task<Account> GetById(int id);
        Task<List<AccountResponse>> GetIdAndUsernameByRole(int roleId);
        Task<PagedList<AccountResponse>> GetPaging(AccountSearch search);
        //Task<AuthenticationResponse> RefreshToken(string refreshToken, HttpContext context);
        Task<bool> SendEmailPassword(string email);
        Task<Account> SigninCookieAsync(string username, string password);
        Task<AuthenticationResponse> SignupAsync(CreateAccountRequest request);
        Task<bool> Create(CreateAccountRequest request);
        Task<bool> Update(int id, UpdateAccountRequest request);
        Task<AccountProfile> GetAccountProfile();
        Task<bool> UpdateProfile(AccountProfile request);
        Task<List<PricingResponse>> GetPricingSeparate(List<int> accountIds);
        Task<bool> CreatePricingSeparate(int accountId, CreatePricingSeparateRequest request);
        Task<bool> UpdatePricingSeparate(int accountId, PricingSeparate request);
        Task<bool> DeletePricingSeparate(int accountId, int id);
        Task<List<Account>> GetSales(int id);
        Task<Account> GetUserByUsernameChat(string username, string token);
        Task<List<AccountWarehouseSupervisor>> GetAccountWarehouseSupervisorsBySaleId(int id);
        Task<bool> IsUserSession();
        Task<AccountHomeResponse> GetDashboard();
        Task<List<AccountResponse>> getListAccount(AccountSearch search);
    }
}
