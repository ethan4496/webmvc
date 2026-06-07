using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class TransportationHistory : BaseEntity
    {
        public string Content { get; set; }

        public int TransportationId { get; set; }
    }
}
