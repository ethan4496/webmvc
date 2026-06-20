namespace WebMVC.Models.Requests.Updates
{
    public class UpdateTransportationUserUploadImageRequest
    {
        public string Barcode { get; set; }
        public List<IFormFile> Images { get; set; }

    }
}
