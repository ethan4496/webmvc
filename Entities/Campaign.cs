using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Campaign : BaseEntity
    {
        public string Name { get; set; }
        public int EmailTemplateId { get; set; } 
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SentAt { get; set; }
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        
        [ForeignKey(nameof(EmailTemplateId))]
        public virtual EmailTemplate EmailTemplate { get; set; }

    }
}
