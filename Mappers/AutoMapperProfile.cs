
using AutoMapper;
using WebMVC.Entities;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;

namespace WebMVC.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Transportation, TransportationResponse>();
            CreateMap<Transportation, TrackingResponse>();
            CreateMap<UpdateTransportationRequest, Transportation>();
            CreateMap<UpdateTransportationAtOutOfStockManageRequest, Transportation>();

            CreateMap<CreateBigPackageRequest, BigPackage>();
            CreateMap<BigPackage, BigPackageResponse>();
            CreateMap<UpdateBigPackageRequest, BigPackage>();

            CreateMap<UpdatePricingRequest, Pricing>();

            CreateMap<Account, AccountResponse>();
            CreateMap<CreateAccountRequest, Account>();
            CreateMap<UpdateAccountRequest, Account>();

            CreateMap<OutOfStock, ManageOutOfStockResponse>();
            CreateMap<OutOfStock, OutOfStockResponse>().ReverseMap();

            CreateMap<UpdateWebConfigurationRequest, WebConfiguration>();

        }
    }
}
