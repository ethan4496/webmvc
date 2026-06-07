using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
    }
}
