namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class CreateCampaignRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    [Required]
    public int EmailTemplateId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Subject { get; set; }

    [Required]
    public string Body { get; set; }
    [Required]
    public DateTime SentAt { get; set; }

    public string Status { get; set; } = "active";
}