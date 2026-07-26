namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class ReportStatistic
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public int? CampaignId {get; set;}
}