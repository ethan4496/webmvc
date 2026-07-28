namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class CreateContactListApiRequest
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public CreateContactListRequest request { get; set; }
}