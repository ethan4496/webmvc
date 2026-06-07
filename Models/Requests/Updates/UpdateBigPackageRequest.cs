namespace WebMVC.Models.Requests.Updates
{
    public class UpdateBigPackageRequest
    {
        public string Name { get; set; }
        public string Partner { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public int Quantity { get; set; }
        public int Status { get; set; }
    }
}
