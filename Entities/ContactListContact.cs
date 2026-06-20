using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class ContactListContact
    {
        public int ContactId { get; set; }

        public int ContactListId { get; set; }

        public Contact Contact { get; set; }

        public ContactList ContactList { get; set; }
    }
}

