using WebMVC.Entities;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class BigPackageResponse
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public string Partner { get; set; }
        public int Status { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public int Quantity { get; set; }
        public int? ShipId { get; set; }
        public decimal? Total { get; set; }
        public decimal? TotalExpense { get; set; }

        public string StatusName
        {
            get
            {
                return EBigPackageStatusName.GetStatusName(Status);
            }
        }

        public List<BigPackageHistory> BigPackageHistories { get; set; }
        public List<ExpenseResponse> Expenses { get; set; }
        // public PagedList<OutOfStockResponse> OutOfStockResponse {get; set;}
    }
}
