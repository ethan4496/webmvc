using System.ComponentModel.DataAnnotations.Schema;

namespace WebMVC.Entities
{
    public class PostCategory
    {
        public int PostId { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; }
    }
}
