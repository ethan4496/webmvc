using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class EmailTemplate : BaseEntity
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public virtual ICollection<Campaign> Campaigns { get; set; }
    }
}
