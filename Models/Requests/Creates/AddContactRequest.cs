namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class AddContactRequest
{
    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(255)]
    public string LastName { get; set; }

    // [Required]
    // public IFormFile File { get; set; }

    [Required]
    public string Email { get; set; }
}
