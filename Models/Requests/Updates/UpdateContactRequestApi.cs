namespace WebMVC.Models.Requests.Updates;
using System.ComponentModel.DataAnnotations;

public class UpdateContactRequestApi
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public UpdateContactRequest request { get; set; }
}