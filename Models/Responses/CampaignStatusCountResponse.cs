namespace WebMVC.Models.Responses
{
    public class CampaignStatusCountResponse
    {
        public int active { get; set; }
        public int inactive { get; set; }
        public int pending { get; set; }
        public int failed { get; set; }
        public int sent { get; set; }
    }
}
