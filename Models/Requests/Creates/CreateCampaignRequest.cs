namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class CreateCampaignRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    [MaxLength(500)]
    public string Subject { get; set; }

    [Required]
    public string Body { get; set; }
    public string EmailSent { get; set; }
    [Required]
    public DateTime SendAt { get; set; }
    public string Status { get; set; } = "active";
    public int ContactId { get; set; }
    public int TemplateId { get; set; }
}