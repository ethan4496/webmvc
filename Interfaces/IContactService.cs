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
        Task<List<ContactList>> GetAll();
        Task CreateAsync(CreateContactListRequest request);
        Task<Contact> addContact(AddContactRequest request);
        Task<ContactListResponse> GetContactById(int id);
        Task SaveAsync(int id, AddContactListRequest request);
        Task DeleteAsync(int id);
        Task DeleteContact(int id, int ContactListId);

    }
}
