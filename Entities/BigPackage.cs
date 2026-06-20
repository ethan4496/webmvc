using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class BigPackage : BaseEntity
    {
        public string Name { get; set; }
        public string Partner { get; set; }
        public int Status { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public int Quantity { get; set; }
    }
}
