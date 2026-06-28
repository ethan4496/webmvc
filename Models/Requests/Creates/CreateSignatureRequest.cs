namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class CreateSignatureRequest
{

    [Required]
    public string Body { get; set; }
    public string LogoPath { get; set; }

}