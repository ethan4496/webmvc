using WebMVC.Entities;

namespace WebMVC.Models.ViewModels
{
    public class CreateCampaignViewModel
    {
        public Campaign? Campaign {get; set;}
        public List<EmailTemplate> Templates { get; set; }
        public List<ContactList> ContactLists { get; set; }
    }
}