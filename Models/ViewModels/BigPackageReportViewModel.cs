using WebMVC.Entities;
using WebMVC.Models.Responses;

namespace WebMVC.Models.ViewModels
{
    public class BigPackageReportViewModel
    {
        public IList<BigPackageResponse> Items { get; set; } = new List<BigPackageResponse>();
        public List<Warehouse> Warehouses { get; set; } = new();
    }
}
