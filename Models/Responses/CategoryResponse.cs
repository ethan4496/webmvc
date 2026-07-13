using WebMVC.Entities;

namespace WebMVC.Models.Responses
{
    public class CategoryResponse : Category
    {
        public string ParentName { get; set; }
    }
}
