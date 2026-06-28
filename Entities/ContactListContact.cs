using DocumentFormat.OpenXml.Office2010.Excel;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class ContactListContact
    {
        public int Id {get; set;}
        public int ContactId { get; set; }

        public int ContactListId { get; set; }

        public Contact Contact { get; set; }

        public ContactList ContactList { get; set; }
    }
}

