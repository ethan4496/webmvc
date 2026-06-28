using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Campaign : BaseEntity
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SendAt { get; set; }
        public int TemplateId { get; set; }
        public int ContactId { get; set; }
        public string EmailSent { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        
        [ForeignKey(nameof(TemplateId))]
        public virtual EmailTemplate EmailTemplate { get; set; }

    }
}
