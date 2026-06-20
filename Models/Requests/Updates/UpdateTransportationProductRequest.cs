using WebMVC.Entities;

namespace WebMVC.Models.Requests.Updates
{
    public class UpdateTransportationProductRequest : TransportationProduct
    {
        public IFormFile ImageFile { get; set; }
    }
}
