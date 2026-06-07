using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class AccountWarehouseSupervisor : BaseEntity
    {
        public int SaleId { get; set; }
        public int WarehouseStaffId { get; set; }
    }
}
