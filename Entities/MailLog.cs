using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class MailLog : BaseEntity
    {
        public int CampaignId { get; set; }
        public int ContactId { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }

    }
}
