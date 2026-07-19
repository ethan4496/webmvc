using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Post : BaseEntity
    {
        public string Title { get; set; }
        // public string Status { get; set;}
        public string Excerpt { get; set; }
        public string? Slug { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }

        public virtual ICollection<PostCategory> PostCategories { get; set; }
    }
}
