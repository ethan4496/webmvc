namespace WebMVC.Models.Requests.Creates
{
    public class CreateBigPackageRequest
    {
        public string Name { get; set; }
        public string Partner { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public int Quantity { get; set; }
        public List<int> TransporationIds { get; set; } = new List<int>();
    }
}
