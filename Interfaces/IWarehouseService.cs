using WebMVC.Entities;

namespace WebMVC.Interfaces
{
    public interface IWarehouseService
    {
        //Task<List<Warehouse>> GetSpecialShip();
        Task<List<Warehouse>> GetWarehousesByStatus(int status);
        Task<List<Warehouse>> GetWarehousesByType(int type);
    }
}
