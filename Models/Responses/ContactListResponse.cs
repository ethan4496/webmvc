using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class ContactListResponse : ContactList
    {
        public List<Contact> Contacts { get; set; } = new();
    }
}
