namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class DeleteContactApi
{
    public int UserId { get; set; }
    public string Key { get; set; }
    public int ContactListId { get; set; }
}