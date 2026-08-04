using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class CampaignResponse : Campaign
    {
        public string AccountName {get; set;}
        public string ContactListName { get; set; }
        public string TemplateName { get; set; }
        public int ContactEmailCount { get; set; }
        public int EmailTrackingCount { get; set; }
        public string StatusBadge
        {
            get
            {
                var (label, colorClass) = Status switch
                {
                    "pending" => ("Pending", "bg-danger"),
                    "active" => ("Active", "bg-success"),
                    "inactive" => ("Inactive", "bg-danger"),
                    "sent" => ("Sent", "bg-success"),
                    _ => (Status, "bg-secondary")
                };

                return $"<span class=\"badge {colorClass}\">{label}</span>";
            }
        }
        public string SendAtStr
        {
            get
            {
                return SendAt.ToString("dd/MM/yyyy HH:mm");
            }
        }
    }
}
