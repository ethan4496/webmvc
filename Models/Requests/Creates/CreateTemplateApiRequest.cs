namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class CreateTemplateApiRequest
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public CreateTemplateRequest request { get; set; }
}