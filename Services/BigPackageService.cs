using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Services
{
    public class BigPackageService : IBigPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;
        private readonly ITransportationService _transportationService;
        private readonly ISignalRService _signalRService;

        public BigPackageService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextService httpContextService, ITransportationService transportationService, ISignalRService signalRService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextService = httpContextService;
            _transportationService = transportationService;
            _signalRService = signalRService;
        }

        public async Task<bool> CreateBigPackage(CreateBigPackageRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Name == request.Name.Trim());
            if (bigPackage != null)
                throw new AppException("Tên bao đã tồn tại");
            return await CreateBigPackageForApi(request, currentDate, currentAccount);
        }

        public async Task<bool> CreateBigPackageForApi(CreateBigPackageRequest request, DateTime currentDate, Account currentAccount)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var bigPackage = _mapper.Map<BigPackage>(request);
                bigPackage.Status = (int)EBigPackageStatus.ExitedFromTQWarehouse;
                await _unitOfWork.Repository<BigPackage>().Add(bigPackage, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();

                await _unitOfWork.Repository<BigPackageHistory>().Add(new BigPackageHistory
                {
                    BigPackageId = bigPackage.Id,
                    Content = string.Format(HistoryContent.TAO_BAO, currentAccount.Username, bigPackage.Name, bigPackage.Quantity, bigPackage.Weight, bigPackage.Volume),

                }, currentDate, currentAccount.Id);

                var transportations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => request.TransporationIds.Contains(x.Id)).ToListAsync();
                var transporationHistories = new List<TransportationHistory>();
                foreach (var transportation in transportations)
                {
                    transportation.BigPackageId = bigPackage.Id;
                    transporationHistories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.GAN_BAO, currentAccount.Username, transportation.Id, request.Name)
                    });

                    int oldTransportationStatus = transportation.Status;
                    int newTransportationStatus = ETransportationStatusName.GetStatusByBigPackageStatus(bigPackage.Status);
                    transportation.Status = newTransportationStatus;
                    await _transportationService.ChangeStatusDateAsync(transportation, currentAccount.Username, currentDate, currentAccount.Id);
                    await _signalRService.SendConfirmNotification($"UpdatedBarcode-{transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");
                }

                _unitOfWork.Repository<Transportation>().UpdateRange(transportations, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<TransportationHistory>().AddRange(transporationHistories, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<bool> DeleteSelected(List<int> ids)
        {
            var bigPackages = await _unitOfWork.Repository<BigPackage>().GetQueryable().Where(x => ids.Contains(x.Id)).ToListAsync();
            var transporations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => bigPackages.Select(b => b.Id).Contains(x.BigPackageId ?? 0)).ToListAsync();
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Repository<BigPackage>().DeleteRange(bigPackages);
                _unitOfWork.Repository<Transportation>().DeleteRange(transporations);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<bool> DeleteFilterd(BigPackageSearch search)
        {
            bool hasFilter =
                search?.Status != null ||
                !string.IsNullOrWhiteSpace(search?.Name) ||
                search?.FromDate != null ||
                search?.ToDate != null;

            if (!hasFilter)
            {
                throw new AppException("Bạn phải chọn ít nhất một tiêu chí lọc trước khi xoá.");
            }
            var bigPackages = await _unitOfWork.Repository<BigPackage>().GetQueryable()
                .Where(x => (search.Status == null || x.Status == search.Status)
                    && (string.IsNullOrEmpty(search.Name) || x.Name.Contains(search.Name))
                    && (search.FromDate == null || x.Created >= search.FromDate)
                    && (search.ToDate == null || x.Created <= search.ToDate)
                ).ToListAsync();
            var transporations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => bigPackages.Select(b => b.Id).Contains(x.BigPackageId ?? 0)).ToListAsync();
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Repository<BigPackage>().DeleteRange(bigPackages);
                _unitOfWork.Repository<Transportation>().DeleteRange(transporations);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<BigPackageResponse> GetById(int id)
        {
            var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            var bigPackageResponse = _mapper.Map<BigPackageResponse>(bigPackage);
            var histories = await _unitOfWork.Repository<BigPackageHistory>().GetQueryable().Where(x => x.BigPackageId == id).OrderByDescending(x => x.Id).ToListAsync();
            bigPackageResponse.BigPackageHistories = histories;
            return bigPackageResponse;
        }

        public async Task<PagedList<BigPackageResponse>> GetPaging(BigPackageSearch search)
        {
            var query = _unitOfWork.Repository<BigPackage>().GetQueryable()
                .Where(x => (search.Status == null || x.Status == search.Status)
                    && (string.IsNullOrEmpty(search.Name) || x.Name.Contains(search.Name))
                    && (search.FromDate == null || x.Created >= search.FromDate)
                    && (search.ToDate == null || x.Created <= search.ToDate)
                );

            int total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Id)   // phải đặt trước
                .Skip((search.PageIndex - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            return new PagedList<BigPackageResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = _mapper.Map<List<BigPackageResponse>>(data)
            };
        }

        public async Task<bool> Update(int id, UpdateBigPackageRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            return await UpdateForApi(id, request, currentDate, currentAccount);
        }
        public async Task<bool> UpdateForApi(int id, UpdateBigPackageRequest request, DateTime currentDate, Account currentAccount)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
                int oldStatus = bigPackage.Status;
                if (request.Status == bigPackage.Status
                    && request.Quantity == bigPackage.Quantity
                    && request.Weight == bigPackage.Weight
                    && request.Volume == bigPackage.Volume
                    && request.Name == bigPackage.Name
                    && request.Partner == bigPackage.Partner
                    )
                {
                    return true;
                }

                if (request.Quantity != bigPackage.Quantity
                    || request.Weight != bigPackage.Weight
                    || request.Volume != bigPackage.Volume
                    || request.Name != bigPackage.Name
                    || request.Partner != bigPackage.Partner)
                {
                    await _unitOfWork.Repository<BigPackageHistory>().Add(new BigPackageHistory
                    {
                        BigPackageId = bigPackage.Id,
                        Content = string.Format(HistoryContent.DOI_THONG_TIN_BAO, currentAccount.Username, request.Name, request.Partner, request.Quantity, request.Weight, request.Volume),

                    }, currentDate, currentAccount.Id);
                }
                _mapper.Map(request, bigPackage);
                _unitOfWork.Repository<BigPackage>().Update(bigPackage, currentDate, currentAccount.Id);
                if (oldStatus != bigPackage.Status)
                {
                    await _unitOfWork.Repository<BigPackageHistory>().Add(new BigPackageHistory
                    {
                        BigPackageId = bigPackage.Id,
                        Content = string.Format(HistoryContent.DOI_TRANG_THAI_BAO, currentAccount.Username, bigPackage.Name, EBigPackageStatusName.GetStatusName(oldStatus), EBigPackageStatusName.GetStatusName(request.Status)),

                    }, currentDate, currentAccount.Id);
                    var transportations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => x.BigPackageId == id).ToListAsync();
                    var transporationHistories = new List<TransportationHistory>();
                    switch (bigPackage.Status)
                    {
                        case (int)EBigPackageStatus.Cancel:
                            foreach (var transportation in transportations)
                            {
                                transportation.BigPackageId = null;
                                transporationHistories.Add(new TransportationHistory
                                {
                                    TransportationId = transportation.Id,
                                    Content = string.Format(HistoryContent.BO_GAN_BAO, currentAccount.Username, transportation.Barcode, bigPackage.Name)
                                });
                                await _signalRService.SendConfirmNotification($"UpdatedBarcode-{transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tải lại dữ liệu");

                            }
                            _unitOfWork.Repository<Transportation>().UpdateRange(transportations, currentDate, currentAccount.Id);
                            await _unitOfWork.Repository<TransportationHistory>().AddRange(transporationHistories, currentDate, currentAccount.Id);
                            break;
                        case (int)EBigPackageStatus.Completed:
                            break;
                        default:
                            foreach (var transportation in transportations)
                            {
                                int oldTransportationStatus = transportation.Status;
                                int newTransportationStatus = ETransportationStatusName.GetStatusByBigPackageStatus(request.Status);
                                transportation.Status = newTransportationStatus;
                                await _transportationService.ChangeStatusDateAsync(transportation, currentAccount.Username, currentDate, currentAccount.Id);

                                transporationHistories.Add(new TransportationHistory
                                {
                                    TransportationId = transportation.Id,
                                    Content = string.Format(HistoryContent.DOI_TRANG_THAI_DON_THEO_BAO, currentAccount.Username, transportation.Barcode, ETransportationStatusName.GetStatusName(oldStatus), ETransportationStatusName.GetStatusName(newTransportationStatus), bigPackage.Name)
                                });
                                await _signalRService.SendConfirmNotification($"UpdatedBarcode-{transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");

                            }
                            _unitOfWork.Repository<Transportation>().UpdateRange(transportations, currentDate, currentAccount.Id);
                            await _unitOfWork.Repository<TransportationHistory>().AddRange(transporationHistories, currentDate, currentAccount.Id);

                            break;
                    }

                }
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }
    }
}
