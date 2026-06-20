using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class TransportationProduct : BaseEntity
    {
        public int TransportationId { get; set; }
        public string Name { get; set; }
        public string Dimensions { get; set; }
        public int Quantity { get; set; }
        public string OtherInfor { get; set; }
        public string Image { get; set; }
    }
}
