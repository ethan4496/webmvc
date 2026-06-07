namespace WebMVC.Models.Requests.Searchs
{
    public class PagingSearch
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; }

        public DateTime? FromDate { get; set; }
        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set => _toDate = value?.Date.AddDays(1).AddTicks(-1);
        }
    }
}
