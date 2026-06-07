using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class TransportationOutOfStock : BaseEntity
    {
        public int TransportationId { get; set; }
        public int OutOfStockId { get; set; }
    }
}
