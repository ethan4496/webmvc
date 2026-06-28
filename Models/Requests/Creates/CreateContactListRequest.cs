namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class CreateContactListRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    public IFormFile File { get; set; }
}
