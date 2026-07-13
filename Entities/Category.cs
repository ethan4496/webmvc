using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
