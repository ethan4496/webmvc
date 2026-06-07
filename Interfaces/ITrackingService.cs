using WebMVC.Entities;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Services;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface ITrackingService
    {
        Task<TransportFeeResponse> Create(CreateTrackingRequest request);
        Task<TransportFeeResponse> CreateForAPI(CreateTrackingRequest request, Account currentAccount);
        Task<TransportFeeResponse> GetByBarcode(string barcode);
        Task<string> GetInfo(string barcode);
        Task<TrackingResponse> TrackingByBarcode(string barcode);
        Task<bool> UpdateTransportationProduct(UpdateTransportationProductRequest request);
    }
}
