using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Services
{
    public class PricingService : IPricingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;

        public PricingService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextService httpContextService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextService = httpContextService;
        }

        public async Task<bool> Create(Pricing request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            await _unitOfWork.Repository<Pricing>().Add(request, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var pricing = await _unitOfWork.Repository<Pricing>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            _unitOfWork.Repository<Pricing>().Delete(pricing);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<PagedList<PricingResponse>> GetPaging(PricingSearch search)
        {
            var datas = await (from pricing in _unitOfWork.Repository<Pricing>().GetQueryable()
                                  join warehouseFrom in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Reciever) on pricing.FromWarehouseId equals warehouseFrom.Id into warehouseFromJoin
                                  from warehouseFrom in warehouseFromJoin.DefaultIfEmpty()
                                  join warehouseTo in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Destination) on pricing.ToWarehouseId equals warehouseTo.Id into warehouseToJoin
                                  from warehouseTo in warehouseToJoin.DefaultIfEmpty()
                                  join shippingType in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Shipping) on pricing.ShipId equals shippingType.Id into shippingTypeJoin
                                  from shippingType in shippingTypeJoin.DefaultIfEmpty()
                                  where (search.Type == null || pricing.Type == search.Type)
                                        && (search.FromId == null || pricing.FromWarehouseId == search.FromId)
                                        && (search.ToId == null || pricing.ToWarehouseId == search.ToId)
                                        && (search.ShipId == null || pricing.ShipId == search.ShipId)
                                  select new PricingResponse
                                  {
                                      Id = pricing.Id,
                                      RangeMin = pricing.RangeMin,
                                      RangeMax = pricing.RangeMax,
                                      PricePerUnit = pricing.PricePerUnit,
                                      Type = pricing.Type,
                                      FromWarehouseId = pricing.FromWarehouseId,
                                      ToWarehouseId = pricing.ToWarehouseId,
                                      ShipId = pricing.ShipId,
                                      FromWarehouseName = warehouseFrom.Name,
                                      ToWarehouseName = warehouseTo.Name,
                                      ShipName = shippingType.Name,
                                      Created = pricing.Created,
                                      CreatedBy = pricing.CreatedBy,
                                      Updated = pricing.Updated,
                                      UpdateBy = pricing.UpdateBy,
                                  })
                                .OrderBy(x => x.Type)
                                .ThenBy(x => x.FromWarehouseId)
                                .ThenBy(x => x.ShipId)
                                .Skip((search.PageIndex - 1) * search.PageSize)
                                .Take(search.PageSize)
                                .ToListAsync();

            int total = await _unitOfWork.Repository<Pricing>().GetQueryable()
                            .Where(x => (search.Type == null || x.Type == search.Type)
                                       && (search.FromId == null || x.FromWarehouseId == search.FromId)
                                       && (search.ToId == null || x.ToWarehouseId == search.ToId)
                                       && (search.ShipId == null || x.ShipId == search.ShipId))
                            .CountAsync();

            return new PagedList<PricingResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = datas
            };
        }

        public async Task<bool> Update(int id, UpdatePricingRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var pricing = await _unitOfWork.Repository<Pricing>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            _mapper.Map(request, pricing);
            _unitOfWork.Repository<Pricing>().Update(pricing, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
