namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class CreateSignatureApiRequest
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public CreateSignatureRequest request { get; set; }
}