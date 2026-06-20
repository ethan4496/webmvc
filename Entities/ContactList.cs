using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class ContactList : BaseEntity
{
    public string Name { get; set; }

    public ICollection<ContactListContact> ContactListContacts { get; set; }
}
}
