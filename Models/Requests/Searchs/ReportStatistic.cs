namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class ReportStatistic
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public int? CampaignId {get; set;}
    public string? Status {get; set;}
    public DateTime? FromDate {get; set;}
    public DateTime? ToDate {get; set;}
}