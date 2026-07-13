using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Post : BaseEntity
    {
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string Content { get; set; }
        public int CategoryId { get; set; }
        public string Image { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; }
    }
}
