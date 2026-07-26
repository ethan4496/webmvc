using DocumentFormat.OpenXml.Office2010.Excel;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class EmailTracking
    {
        public int Id {get; set;}
        public int CampaignId { get; set; }

        public string RecipientEmail { get; set; }
        public int OpenCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

