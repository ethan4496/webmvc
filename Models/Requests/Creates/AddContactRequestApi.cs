namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class AddContactRequestApi
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public AddContactRequest request { get; set; }
}