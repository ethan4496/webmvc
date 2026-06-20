namespace WebMVC.Models.Requests.Searchs
{
    public class PriceImportFilter
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Name { get; set; }

        public string? Hscode { get; set; }

        public string? From { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }
    }
}
