using WebMVC.Entities;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IContactService
    {
        Task<PagedList<ContactListResponse>> GetPaging(ContactSearch search);
        Task CreateAsync(CreateContactListRequest request);
        Task addContact(int id, AddContactRequest request);
        Task<Campaign> GetCampaignById(int id);
        Task DeleteAsync(int id);

    }
}
