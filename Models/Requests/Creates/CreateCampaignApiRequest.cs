namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class CreateCampaignApiRequest
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public CreateCampaignRequest request { get; set; }
}