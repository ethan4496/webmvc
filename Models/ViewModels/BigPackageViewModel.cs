using WebMVC.Entities;
using WebMVC.Models.Responses;

public class BigPackageViewModel
{
    public BigPackageResponse BigPackage { get; set; }
    public List<Warehouse> Warehouses { get; set; }
}