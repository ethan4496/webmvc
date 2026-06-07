namespace WebMVC.Models.Requests.Creates
{
    public class CreateTransportationRequest
    {
        public int WarehouseFrom { get; set; }
        public int WarehouseTo { get; set; }
        public int TransportMethod { get; set; }
        public List<CreateTransportationItem> Items { get; set; }
    }

    public class CreateTransportationItem
    {
        public string Barcode { get; set; }
        // public string? hscode { get; set; }
        public string Note { get; set; }
        public List<IFormFile> Images { get; set; }
        public string ImageUrl { get; set; }
        public int VoucherId { get; set; }

        public double? UserUploadWeight { get; set; }
        public double? UserUploadVolume { get; set; }
        public int? UserUploadQuantity { get; set; }
        public List<CreateTransportationProduct> Products { get; set; }
    }

    public class CreateTransportationProduct
    {
        public string Name { get; set; }
        public string Dimensions { get; set; }
        public int Quantity { get; set; }
        public string OtherInfor { get; set; }
        public IFormFile Image { get; set; }
        public string ImageUrl { get; set; }
    }
}
