namespace WebMVC.Models.Requests.Creates;
using System.ComponentModel.DataAnnotations;

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public IFormFile Image { get; set; }

    public int? ParentId { get; set; }
}
