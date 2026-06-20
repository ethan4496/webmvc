using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Contact : BaseEntity
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public ICollection<ContactListContact> ContactListContacts { get; set; }
}
}
