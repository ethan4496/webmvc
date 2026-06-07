using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Interfaces;

namespace WebMVC.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextService _httpContextService;

        public WarehouseService(IUnitOfWork unitOfWork, IHttpContextService httpContextService)
        {
            _unitOfWork = unitOfWork;
            _httpContextService = httpContextService;
        }

        public async Task<List<Warehouse>> GetWarehousesByStatus(int status)
        {
            //var query = _unitOfWork.Repository<Warehouse>().GetQueryable();

            //var currentAccount = await _httpContextService.GetCurrentAccount();

            //List<int> specialShipIds = new();
            //if (currentAccount != null && !string.IsNullOrWhiteSpace(currentAccount.SpecialShipId))
            //{
            //    specialShipIds = currentAccount.SpecialShipId
            //        .Split(',')
            //        .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
            //        .Where(id => id.HasValue)
            //        .Select(id => id.Value)
            //        .ToList();
            //}

            //// Gộp điều kiện theo logic bạn muốn
            //query = query.Where(x =>
            //    (status < 0 || (x.Status == status || (specialShipIds.Count > 0 && specialShipIds.Contains(x.Id))))            
            //);

            //return await query.OrderBy(x => x.Type).ToListAsync();
            return await _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => (status < 0 || x.Status == status)).OrderBy(x => x.Id).ToListAsync();

        }


        public async Task<List<Warehouse>> GetWarehousesByType(int type)
        {
            return await _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == type).OrderBy(x => x.Id).ToListAsync();
        }

        //public async Task<List<Warehouse>> GetSpecialShip()
        //{
        //    return await _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Shipping && x.Status == (int)EWarehouseStatus.Special).OrderBy(x => x.Id).ToListAsync();
        //}
    }
}
