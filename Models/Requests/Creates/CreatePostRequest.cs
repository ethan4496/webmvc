namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class CreatePostRequest
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Excerpt { get; set; }
    public string? Slug { get; set; }
    public string Content { get; set; }

    public IFormFile Image { get; set; }

    public List<int> CategoryIds { get; set; } = new();
}
