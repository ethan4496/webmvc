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
        Task<ResponseClass> GetPagingApi(ContactListApiSearch search);
        Task<List<ContactList>> GetAll();
        Task CreateAsync(CreateContactListRequest request);
        Task<ResponseClass> CreateApiAsync(CreateContactListApiRequest request);
        Task<Contact> addContact(AddContactRequest request);
        Task<ResponseClass> AddEmailContactList(int id, AddContactRequestApi request);
        Task<ContactListResponse> GetContactById(int id, string email = null);
        Task<ResponseClass> GetContactList(int id, AppUser request);
        Task<ResponseClass> GetEmailContactList(int id, EmailContactListSearch request);
        Task SaveAsync(int id, AddContactListRequest request);
        Task<ResponseClass> UpdateContactList(int id, CreateContactListApiRequest request);
        Task DeleteAsync(int id);
        Task<ResponseClass> DeleteApiAsync(int id, AppUser appUser);
        Task DeleteContact(int id, int ContactListId);
        Task<ResponseClass> DeleteContactApi(int id,  DeleteContactApi appUser);
        Task EditContactAsync(UpdateContactRequest request);
        Task<ResponseClass> UpdateContactApi(int id, UpdateContactRequestApi request);

    }
}
