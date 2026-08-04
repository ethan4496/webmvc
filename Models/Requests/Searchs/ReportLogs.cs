namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class ReportLogs
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public int? campaignId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate {get; set;}
    public DateTime? ToDate {get; set;}
    public int pageIndex { get; set; } = 1;
    public int pageSize { get; set; }
}