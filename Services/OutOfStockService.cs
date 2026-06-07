using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using SelectPdf;
using Serilog;
using System.Text;
using WebMVC.BackgroundWorkers;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;
using static WebMVC.Services.ZaloAPIService;

namespace WebMVC.Services
{
    public class OutOfStockService : IOutOfStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;
        private readonly ITransportationService _transportationService;
        private readonly IWebHostEnvironment _env;
        private readonly ISendEmailService _sendEmailService;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly INotificationService _notificationService;
        private readonly IZaloAPIService _zaloAPIService;

        public OutOfStockService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextService httpContextService,
            ITransportationService transportationService, IWebHostEnvironment env,
            ISendEmailService sendEmailService, IBackgroundTaskQueue backgroundTaskQueue,
            INotificationService notificationService, IZaloAPIService zaloAPIService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextService = httpContextService;
            _transportationService = transportationService;
            _env = env;
            _sendEmailService = sendEmailService;
            _backgroundTaskQueue = backgroundTaskQueue;
            _notificationService = notificationService;
            _zaloAPIService = zaloAPIService;
        }
        public async Task<bool> UpdateIsPrintTemp(int id)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            if (outOfStock.IsPrintTemp == true)
                return true;
            outOfStock.IsPrintTemp = true;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }
        public async Task<string> GetDeliveryNoteUnpaidOfCurrentAccount()
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var ids = await _unitOfWork.Repository<OutOfStock>().GetQueryable().Where(x => x.AccountId == currentAccount.Id && x.StatusPayment != (int)EPaymentOutOfStockStatus.Paied && x.Status != (int)EOutOfStockStatus.Cancel).Select(x => x.Id).ToListAsync();
            return await RenderDeliveryNote(ids);
        }
        public async Task<int> Create(CreateOutOfStockRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var currentAccount = await _httpContextService.GetCurrentAccount();
                var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.AccountId);
                var histories = new List<TransportationHistory>();
                var transportationOutOfStocks = new List<TransportationOutOfStock>();
                var transportations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => request.Barcodes.Contains(x.Barcode) && x.Status == (int)ETransportationStatus.ArrivedAtVNWarehouse && x.AccountId == request.AccountId && x.IsOutStock != true).ToListAsync();
                if (transportations?.Count != request.Barcodes.Count)
                {
                    throw new AppException("Số lượng mã chưa xuất kho không khớp với dữ liệu");
                }
                decimal totalPriceVND = transportations.Sum(x => x.TotalPriceVND);
                var outOfStock = new OutOfStock()
                {
                    AccountId = request.AccountId,
                    Status = (int)EOutOfStockStatus.New,
                    StatusPayment = (int)EPaymentOutOfStockStatus.New,
                    PostOffice = customerAccount.PostOffice,
                    DeliveryMethod = customerAccount.DeliveryMethod,
                    DeliveryInfo = $"{customerAccount.FullName} \nSĐT: {customerAccount.Phone} \nĐịa chỉ: {customerAccount.Address}",
                    IsRequest = false,
                    IsSend = false,
                    TotalPriceVND = totalPriceVND
                };
                await _unitOfWork.Repository<OutOfStock>().Add(outOfStock, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
                foreach (var transportation in transportations)
                {
                    //var transportationOuthttps://scontent.fhan4-2.fna.fbcdn.net/v/t39.30808-6/646370138_1435845555253764_3334203973489270430_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=1&ccb=1-7&_nc_sid=13d280&_nc_ohc=otBFC-FtmzkQ7kNvwHV-SYV&_nc_oc=AdmPL8hNtp6HkaoGXQAK7UdoUGEx6cRlEnNY-ONoa6VDUyuzTgPQMWQDDrBz3-dj8M0&_nc_zt=23&_nc_ht=scontent.fhan4-2.fna&_nc_gid=DZAfwgkVRQRpLnpBpVBhhQ&_nc_ss=8&oh=00_AfzCCOaE-5A5m5-IckPT9RFwJP0ZYHRFYdmV_Q0SMxx4uQ&oe=69B0270BOfStock = await _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable().FirstOrDefaultAsync(x => x.TransportationId == transportation.Id);
                    //if (transportationOutOfStock != null)
                    //{
                    //    var existOutOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportationOutOfStock.OutOfStockId);
                    //    existOutOfStock.Status = (int)EOutOfStockStatus.Cancel;
                    //    _unitOfWork.Repository<OutOfStock>().Update(existOutOfStock, currentDate, currentAccount.Id);
                    //}
                    transportation.IsOutStock = true;
                    transportationOutOfStocks.Add(new TransportationOutOfStock
                    {
                        TransportationId = transportation.Id,
                        OutOfStockId = outOfStock.Id
                    });
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.TAO_XUAT_KHO, currentAccount.Username, outOfStock.Id)
                    });
                }
                _unitOfWork.Repository<Transportation>().UpdateRange(transportations, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<TransportationOutOfStock>().AddRange(transportationOutOfStocks, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                var notification = new Notification
                {
                    Title = "Phiếu xuất kho",
                    Content = $"{currentAccount.Username} đã tạo phiên xuất kho #{outOfStock.Id}",
                    WebUrl = $"/out-of-stock/{outOfStock.Id}",
                    Type = (int)ENotificationType.Order,
                    IsStaff = true,
                };
                await _notificationService.SendNotification(notification, currentDate, currentAccount.Id,
                    staffId: customerAccount.SaleId ?? 0, staffRoleIds: new List<int> { (int)ERoleId.Admin, (int)ERoleId.Manager, (int)ERoleId.VNWarehouseStaff, });
                var customerNotification = new Notification
                {
                    Title = "Phiếu xuất kho",
                    Content = $"Bạn có phiên xuất kho mới #{outOfStock.Id}",
                    WebUrl = $"/out-of-stock",
                    Type = (int)ENotificationType.Order,
                    IsStaff = false
                };
                await _notificationService.SendNotification(customerNotification, currentDate, currentAccount.Id,
                    customerId: customerAccount.Id);

                return outOfStock.Id;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<PagedList<OutOfStockResponse>> GetPaging(OutOfStockSearch search)
        {
            var loggedModel = _httpContextService.GetLoggedModel();
            if (loggedModel.IsStaff == 0)
            {
                search.AccountId = loggedModel.Id;
            }
            var query = _unitOfWork.Repository<OutOfStock>().GetQueryable()
                                        .Where(x => x.Status != (int)EOutOfStockStatus.Cancel
                                            && (x.IsSend == true || x.Status == (int)EOutOfStockStatus.Done)
                                            && (search.AccountId == null || x.AccountId == search.AccountId)
                                            && (search.Id == null || x.Id == search.Id)
                                            && (search.Status == null || x.Status == search.Status)
                                            && (search.StatusPayment == null || x.StatusPayment == search.StatusPayment)
                                            && (search.FromDate == null || x.Created >= search.FromDate)
                                            && (search.ToDate == null || x.Created <= search.ToDate)
                                        );
            int total = await query.CountAsync();
            var outOfStocks = await query.OrderByDescending(x => x.Id)
                                 .Skip((search.PageIndex - 1) * search.PageSize)
                                 .Take(search.PageSize)
                                 .ToListAsync();
            var outOfStockResponses = _mapper.Map<List<OutOfStockResponse>>(outOfStocks);
            return new PagedList<OutOfStockResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = outOfStockResponses
            };
        }
        public async Task<PagedList<ManageOutOfStockResponse>> GetPagingManage(OutOfStockSearch search)
        {
            var loggedModel = _httpContextService.GetLoggedModel();
            if (loggedModel.PostOffice != "")
            {
                search.PostOffice = loggedModel.PostOffice;
            }
            int? saleId = null;
            List<int> supervisedSaleIds = null;

            switch (loggedModel.RoleId)
            {
                case (int)ERoleId.Sale:
                    saleId = loggedModel.Id;
                    break;

                case (int)ERoleId.VNWarehouseStaff:
                    supervisedSaleIds = await _unitOfWork.Repository<AccountWarehouseSupervisor>()
                        .GetQueryable()
                        .Where(x => x.WarehouseStaffId == loggedModel.Id)
                        .Select(x => x.SaleId)
                        .Distinct()
                        .ToListAsync();
                    break;
            }

            var query = from o in _unitOfWork.Repository<OutOfStock>().GetQueryable()
                        join a in _unitOfWork.Repository<Account>().GetQueryable() on o.AccountId equals a.Id into accJoin
                        from acc in accJoin.DefaultIfEmpty()
                        where (search.AccountId == null || o.AccountId == search.AccountId)
                              && (search.Id == null || o.Id == search.Id)
                              && (search.Username == null || acc.Username.Contains(search.Username))
                              && (search.PostOffice == null || o.PostOffice == search.PostOffice)
                              && (search.Status == null || o.Status == search.Status)
                              && (search.StatusPayment == null ||
                                (search.StatusPayment != 100 && o.StatusPayment == search.StatusPayment) ||
                                (search.StatusPayment == 100 && o.IsRequest == true && o.StatusPayment == (int)EPaymentOutOfStockStatus.New)
                              )
                              && (search.FromDate == null || o.Created >= search.FromDate)
                              && (search.ToDate == null || o.Created <= search.ToDate)
                              && (saleId == null || acc.SaleId == saleId)
                              && (supervisedSaleIds == null || supervisedSaleIds.Contains(acc.SaleId ?? 0))
                              && (string.IsNullOrEmpty(search.Barcode) ||
                                  _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable()
                                    .Any(tos => tos.OutOfStockId == o.Id &&
                                                _unitOfWork.Repository<Transportation>().GetQueryable()
                                                    .Any(t => t.Id == tos.TransportationId && t.Barcode.Contains(search.Barcode)))
                                 )
                        select new ManageOutOfStockResponse
                        {
                            Id = o.Id,
                            AccountId = o.AccountId,
                            Username = acc.Username,
                            Status = o.Status,
                            StatusPayment = o.StatusPayment,
                            Created = o.Created,
                            CreatedBy = o.CreatedBy,
                            TotalPriceVND = o.TotalPriceVND,
                            PostOffice = o.PostOffice,
                            DeliveryMethod = o.DeliveryMethod,
                            DeliveryInfo = o.DeliveryInfo,
                            IsRequest = o.IsRequest,
                            IsSend = o.IsSend,
                            IsPrintTemp = o.IsPrintTemp,
                            DateCallPhone = o.DateCallPhone,
                            DateOutStock = o.DateOutStock,
                            DatePayment = o.DatePayment,
                            AccountPayment = o.AccountPayment,
                        };

            int total = await query.CountAsync();

            var outOfStocks = await query.OrderByDescending(o => o.Id)
                                         .Skip((search.PageIndex - 1) * search.PageSize)
                                         .Take(search.PageSize)
                                         .ToListAsync();

            var outOfStockIds = outOfStocks.Select(o => o.Id).ToList();

            var transportations = await (from tos in _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable()
                                         join t in _unitOfWork.Repository<Transportation>().GetQueryable() on tos.TransportationId equals t.Id
                                         join s in _unitOfWork.Repository<Warehouse>().GetQueryable() on t.ShipId equals s.Id
                                         where outOfStockIds.Contains(tos.OutOfStockId)
                                         select new
                                         {
                                             tos.OutOfStockId,
                                             Transportation = new TransportationOfOutStockResponse
                                             {
                                                 Barcode = t.Barcode,
                                                 DateCompleted = t.DateCompleted,
                                                 Quantity = t.Quantity,
                                                 UnitVolume = t.UnitVolume,
                                                 UnitWeight = t.UnitWeight,
                                                 Volume = t.Volume,
                                                 Weight = t.Weight,
                                                 VoucherInfo = t.VoucherInfo,
                                                 DateArrivedAtVNWarehouse = t.DateArrivedAtVNWarehouse,
                                                 ShipName = s.Name,
                                             }
                                         })
                                         .OrderByDescending(x => x.Transportation.DateArrivedAtVNWarehouse)
                                         .ToListAsync();

            var transportationDict = transportations
                .GroupBy(t => t.OutOfStockId)
                .ToDictionary(g => g.Key, g => g.Select(t => t.Transportation).ToList());

            foreach (var outOfStock in outOfStocks)
            {
                outOfStock.TransportationResponses = transportationDict.ContainsKey(outOfStock.Id)
                    ? transportationDict[outOfStock.Id]
                    : new List<TransportationOfOutStockResponse>();
            }

            return new PagedList<ManageOutOfStockResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = outOfStocks
            };
        }

        public async Task<OutOfStockResponse> GetById(int id)
        {
            var outOfStockResponse = await (from oos in _unitOfWork.Repository<OutOfStock>().GetQueryable()
                                            join account in _unitOfWork.Repository<Account>().GetQueryable() on oos.AccountId equals account.Id into accountJoin
                                            from account in accountJoin.DefaultIfEmpty()
                                            where oos.Id == id
                                            select new OutOfStockResponse
                                            {
                                                Id = oos.Id,
                                                Created = oos.Created,
                                                Status = oos.Status,
                                                StatusPayment = oos.StatusPayment,
                                                Username = account.Username,
                                                TotalPriceVND = oos.TotalPriceVND,
                                                IsRequest = oos.IsRequest,
                                                IsSend = oos.IsSend,
                                            })
                                             .SingleOrDefaultAsync();
            var transportations = await (from toos in _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable()
                                         join transportation in _unitOfWork.Repository<Transportation>().GetQueryable() on toos.TransportationId equals transportation.Id into transporationJoin
                                         from transportation in transporationJoin.DefaultIfEmpty()
                                         where toos.OutOfStockId == id
                                         select new TransportationResponse
                                         {
                                             Id = transportation.Id,
                                             Barcode = transportation.Barcode,
                                             UserNote = transportation.UserNote,
                                             Status = transportation.Status,
                                             Weight = transportation.Weight,
                                             Volume = transportation.Volume,
                                             Quantity = transportation.Quantity,
                                             Currency = transportation.Currency,
                                             UnitWeight = transportation.UnitWeight,
                                             UnitVolume = transportation.UnitVolume,
                                             Surcharge = transportation.Surcharge,
                                             PriceShipping = transportation.PriceShipping,
                                             TotalPriceVND = transportation.TotalPriceVND,
                                             Discount = transportation.Discount,
                                             Created = transportation.Created,
                                         }).OrderByDescending(x => x.Id).ToListAsync();
            outOfStockResponse.TransportationResponses = transportations;
            return outOfStockResponse;
        }


        public async Task<List<TransportationResponse>> GetTransportationsInOutStock(List<int> outStockIds)
        {
            return await (from toos in _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable()
                          join transportation in _unitOfWork.Repository<Transportation>().GetQueryable()
                              on toos.TransportationId equals transportation.Id
                          where outStockIds.Contains(toos.OutOfStockId)
                          select new TransportationResponse
                          {
                              Id = transportation.Id,
                              Barcode = transportation.Barcode,
                              UserNote = transportation.UserNote,
                              Status = transportation.Status,
                              Weight = transportation.Weight,
                              Volume = transportation.Volume,
                              Quantity = transportation.Quantity,
                              Currency = transportation.Currency,
                              UnitWeight = transportation.UnitWeight,
                              UnitVolume = transportation.UnitVolume,
                              Surcharge = transportation.Surcharge,
                              PriceShipping = transportation.PriceShipping,
                              TotalPriceVND = transportation.TotalPriceVND,
                              Discount = transportation.Discount,
                              Created = transportation.Created,
                              OutOfStockId = toos.OutOfStockId,
                          }).ToListAsync();
        }
        public async Task<bool> Export(int id)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            return await ExportForAPI(id, currentAccount);
        }

        public async Task<bool> ExportList(List<int> ids)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            foreach (var id in ids)
            {
                await ExportForAPI(id, currentAccount);
            }
            return true;
        }
        public async Task<bool> ExportForAPI(int id, Account currentAccount)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id && x.Status != (int)EOutOfStockStatus.Done);
                if (outOfStock == null)
                {
                    await _unitOfWork.CommitAsync();
                    return false;
                }
                var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == outOfStock.AccountId);
                outOfStock.Status = (int)EOutOfStockStatus.Done;
                if (outOfStock.DateOutStock == null)
                {
                    outOfStock.DateOutStock = currentDate;
                }
                var transporationOutOfStocks = await _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable().Where(x => x.OutOfStockId == outOfStock.Id).ToListAsync();
                var histories = new List<TransportationHistory>();
                var transportations = new List<Transportation>();
                var bigPackageIds = new List<int>();
                foreach (var transportationOutOfStock in transporationOutOfStocks)
                {
                    var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportationOutOfStock.TransportationId && x.Status != (int)ETransportationStatus.Completed);
                    if (transportation == null)
                        continue;
                    transportation.Status = (int)ETransportationStatus.Completed;
                    await _transportationService.ChangeStatusDateAsync(transportation, currentAccount.Username, currentDate, currentAccount.Id);
                    transportations.Add(transportation);
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportationOutOfStock.TransportationId,
                        Content = string.Format(HistoryContent.XUAT_KHO, currentAccount.Username, transportation.Barcode)
                    });
                    if (transportation.BigPackageId > 0 && !bigPackageIds.Contains(transportation.BigPackageId ?? 0))
                    {
                        bigPackageIds.Add(transportation.BigPackageId ?? 0);
                    }
                }
                _unitOfWork.Repository<Transportation>().UpdateRange(transportations, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();

                var bigPackageHistories = new List<BigPackageHistory>();
                var bigPackages = new List<BigPackage>();
                foreach (var bigPackageId in bigPackageIds)
                {
                    var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Id == bigPackageId && x.Status != (int)EBigPackageStatus.Completed);
                    if (bigPackage == null)
                        continue;
                    var transportationNotCompleteOfBigPackage = await _unitOfWork.Repository<Transportation>().GetQueryable()
                        .Where(x => x.BigPackageId == bigPackageId && x.Status != (int)ETransportationStatus.Completed && x.Status != (int)ETransportationStatus.Cancel)
                        .CountAsync();
                    if (transportationNotCompleteOfBigPackage > 0)
                        continue;
                    int oldStatus = bigPackage.Status;
                    bigPackage.Status = (int)EBigPackageStatus.Completed;
                    bigPackages.Add(bigPackage);
                    bigPackageHistories.Add(new BigPackageHistory
                    {
                        BigPackageId = bigPackage.Id,
                        Content = string.Format(HistoryContent.DOI_TRANG_THAI_BAO, currentAccount.Username, bigPackage.Name, EBigPackageStatusName.GetStatusName(oldStatus), EBigPackageStatusName.GetStatusName((int)EBigPackageStatus.Completed)),
                    });
                }

                _unitOfWork.Repository<BigPackage>().UpdateRange(bigPackages, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<BigPackageHistory>().AddRange(bigPackageHistories, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitAsync();
                var notification = new Notification
                {
                    Title = "Phiếu xuất kho",
                    Content = $"{currentAccount.Username} đã xuất kho phiên #{outOfStock.Id}",
                    WebUrl = $"/out-of-stock/{outOfStock.Id}",
                    Type = (int)ENotificationType.Order,
                    IsStaff = true,
                };
                await _notificationService.SendNotification(notification, currentDate, currentAccount.Id,
                    staffId: customerAccount.SaleId ?? 0, staffRoleIds: new List<int> { (int)ERoleId.Admin, (int)ERoleId.Manager, (int)ERoleId.VNWarehouseStaff, });
                var customerNotification = new Notification
                {
                    Title = "Phiếu xuất kho",
                    Content = $"Phiên #{outOfStock.Id} đã xuất kho",
                    WebUrl = $"/out-of-stock",
                    Type = (int)ENotificationType.Order,
                    IsStaff = false,
                };
                await _notificationService.SendNotification(customerNotification, currentDate, currentAccount.Id,
                    customerId: customerAccount.Id);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<bool> Cancel(int id)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            outOfStock.Status = (int)EOutOfStockStatus.Cancel;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);

            var transportationIds = await _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable().Where(x => x.OutOfStockId == id).Select(x => x.TransportationId).ToListAsync();
            if (transportationIds.Any())
            {
                var transportations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => transportationIds.Contains(x.Id)).ToListAsync();
                foreach (var transportation in transportations)
                {
                    transportation.IsOutStock = false;
                }
                _unitOfWork.Repository<Transportation>().UpdateRange(transportations, currentDate, currentAccount.Id);
            }
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> Paied(int id)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            outOfStock.StatusPayment = (int)EPaymentOutOfStockStatus.Paied;
            outOfStock.IsSend = true;
            if (outOfStock.DatePayment == null)
            {
                outOfStock.DatePayment = currentDate;
                outOfStock.AccountPayment = currentAccount.Username;
            }
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }
        public async Task<bool> SendRequest(List<int> ids)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            return await SendRequestForAPI(ids, currentAccount, currentDate);
        }

        public async Task<bool> SendOutStockNotis(List<int> ids)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var outOfStocks = await _unitOfWork.Repository<OutOfStock>().GetQueryable().Where(x => ids.Contains(x.Id)).ToListAsync();
            if (!outOfStocks.Any())
            {
                throw new AppException("Không tìm thấy phiên xuất kho chưa gửi thông báo");
            }
            foreach (var outOfStock in outOfStocks)
            {
                var customer = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == outOfStock.AccountId);
                string customerUsername = customer.Username, customerEmail = customer.Email;
                var fileHtml = await RenderDeliveryNote(new List<int> { outOfStock.Id });
                outOfStock.IsSend = true;
                _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
                if (await _unitOfWork.SaveAsync() > 0)
                {
                    var customerNotification = new Notification
                    {
                        Title = "Phiếu xuất kho",
                        Content = $"PXK {outOfStock.Id} Thanh Toán Ngay , nhận hàng liền tay!!!",
                        WebUrl = $"/out-of-stock",
                        Type = (int)ENotificationType.Order,
                        IsStaff = false
                    };
                    await _notificationService.SendNotification(customerNotification, currentDate, currentAccount.Id,
                        customerId: customer.Id);
                    SendEmailDeliveryNote(outOfStock.Id, customerUsername, customerEmail, fileHtml);
                }
            }

            return true;
        }


        public async Task<bool> SendRequestForAPI(List<int> ids, Account currentAccount, DateTime currentDate)
        {
            var outOfStocks = await _unitOfWork.Repository<OutOfStock>().GetQueryable().Where(x => ids.Contains(x.Id)).ToListAsync();
            foreach (var outOfStock in outOfStocks)
            {
                outOfStock.IsRequest = true;
                _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
                var notification = new Notification
                {
                    Title = "Thanh toán xuất kho",
                    Content = $"Khách hàng {currentAccount.Username} đã xác nhận thanh toán phiếu xuất kho #{outOfStock.Id}",
                    WebUrl = $"/out-of-stock/{outOfStock.Id}",
                    Type = (int)ENotificationType.Finance,
                    IsStaff = true,
                };
                await _notificationService.SendNotification(notification, currentDate, currentAccount.Id,
                    staffId: currentAccount.SaleId ?? 0, staffRoleIds: new List<int> { (int)ERoleId.Admin, (int)ERoleId.Manager, (int)ERoleId.VNWarehouseStaff, });
            }
            return true;
        }
        public async Task<bool> SendDeliveryNote(int id)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            var customer = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == outOfStock.AccountId);
            string customerUsername = customer.Username, customerEmail = customer.Email;
            var fileHtml = await RenderDeliveryNote(new List<int> { id });
            outOfStock.IsSend = true;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
            if (await _unitOfWork.SaveAsync() > 0)
            {
                SendEmailDeliveryNote(id, customerUsername, customerEmail, fileHtml);
                var template = new TemplateOutStockData()
                {
                    customer_name = customer.Username,
                    id_phieu = id.ToString()
                };
                await _zaloAPIService.SendMessageOutStock(customer.Phone, "389006", template);
            }
            return true;
        }

        public void SendEmailDeliveryNote(int id, string customerUsername, string customerEmail, string fileHtml)
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
            {
                try
                {
                    var converter = new HtmlToPdf();
                    converter.Options.PdfPageSize = PdfPageSize.A4;
                    converter.Options.MarginTop = 0;
                    converter.Options.MarginBottom = 0;
                    converter.Options.MarginLeft = 0;
                    converter.Options.MarginRight = 0;

                    var pdfBytes = converter.ConvertHtmlString(fileHtml);
                    // Đọc nội dung file HTML
                    string emailTemplatePath = Path.Combine(_env.WebRootPath, "templates", "EmailPXKTemplate.html");
                    string emailContent = await File.ReadAllTextAsync(emailTemplatePath);

                    // Thay thế các placeholder bằng dữ liệu thực tế
                    emailContent = emailContent
                        .Replace("{customerUsername}", customerUsername)
                        .Replace("{id}", id.ToString());

                    using (MemoryStream pdfStream = new MemoryStream())
                    {
                        pdfBytes.Save(pdfStream);
                        pdfStream.Position = 0;
                        // Gửi email với tệp đính kèm là PDF
                        _sendEmailService.Send(customerEmail, "Thông báo TPKEXPRESS.COM", emailContent, pdfStream.ToArray(), "phieu-xuat-kho.pdf");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Send delivery note failed PXK-{id}: {ex.Message};");
                }
                finally { }
            });
        }
        public async Task<bool> UpdateDeliveryInfo(int id, string deliveryInfo)
        {
            var currentDate = DateTime.Now;
            var currentAccount = _httpContextService.GetLoggedModel();
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            if (currentAccount.IsStaff == 0 && outOfStock.AccountId != currentAccount.Id)
            {
                throw new AppException("Phiếu xuất kho không cùng tài khoản");
            }
            outOfStock.DeliveryInfo = deliveryInfo;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> UpdateTotalPriceVND(int id, decimal totalPriceVND)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            outOfStock.TotalPriceVND = totalPriceVND;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> UpdatePostOffice(int id, string postOffice)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            if (outOfStock.Status != (int)EOutOfStockStatus.New)
                throw new AppException("Phiếu xuất kho đã hoàn thành");
            outOfStock.PostOffice = postOffice;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
            await _unitOfWork.ExecuteSqlRawAsync($"UPDATE Transportations SET PostOffice = N'{postOffice}', Updated = '{currentDate}', UpdateBy = {currentAccount.Id} WHERE " +
                $"Id IN (SELECT TransportationId FROM TransportationOutOfStocks WHERE OutOfStockId = {id})");
            return await _unitOfWork.SaveAsync() > 0;
        }
        public async Task<bool> UpdateDeliveryMethod(int id, string deliveryMethod)
        {
            var currentDate = DateTime.Now;
            var currentAccount = _httpContextService.GetLoggedModel();
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            if (outOfStock.Status != (int)EOutOfStockStatus.New)
                throw new AppException("Phiếu xuất kho đã hoàn thành");
            if (currentAccount.IsStaff == 0 && outOfStock.AccountId != currentAccount.Id)
            {
                throw new AppException("Phiếu xuất kho không cùng tài khoản");
            }
            outOfStock.DeliveryMethod = deliveryMethod;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<string> RenderDeliveryNote(List<int> ids)
        {
            var outOfStocks = await _unitOfWork.Repository<OutOfStock>().GetQueryable().Where(x => ids.Contains(x.Id)).ToListAsync();
            var outOfStock = outOfStocks.FirstOrDefault();

            var transportationOutOfStocks = await _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable().Where(x => ids.Contains(x.OutOfStockId)).Select(x => x.TransportationId).ToListAsync();
            var transpotations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => transportationOutOfStocks.Contains(x.Id)).OrderByDescending(x => x.DateArrivedAtVNWarehouse).ToListAsync();
            if (!transpotations.Any())
                throw new AppException("Phiếu xuất kho không có đơn");
            var customer = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transpotations.FirstOrDefault().AccountId);
            var shippingType = await _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Shipping).ToListAsync();
            StringBuilder tbody = new StringBuilder();
            decimal totalDecreaseAmount = 0;
            int totalQuantity = 0;
            double totalWeight = 0, totalVolume = 0;
            for (int i = 0; i < transpotations.Count; i++)
            {
                var transpotation = transpotations[i];
                tbody.AppendLine("<tr class=\"text-center\">");
                tbody.Append("<td>" + (i + 1) + "</td>");
                tbody.Append("<td  style=\"text-transform: uppercase; font-weight:bold\" >" + transpotation.Barcode + "</td>");
                tbody.Append("<td style=\"min-width:200px\">" + transpotation.UserNote + "</td>");
                tbody.Append("<td>" + shippingType.Where(x => x.Id == transpotation.ShipId).FirstOrDefault().Name + "</td>");
                tbody.Append("<td>" + transpotation.Quantity + "</td>");
                tbody.Append("<td>" + Math.Round(transpotation.Weight ?? 0, 1) + "</td>");
                tbody.Append("<td>" + string.Format("{0:N0}", transpotation.UnitWeight) + "</td>");
                tbody.Append("<td>" + Math.Round(transpotation.Volume ?? 0, 5) + "</td>");
                tbody.Append("<td>" + string.Format("{0:N0}", transpotation.UnitVolume) + "</td>");
                tbody.Append("<td>" + string.Format("{0:N0}", transpotation.Surcharge) + "</td>");
                tbody.Append("<td>" + string.Format("{0:N0}", transpotation.Discount) + "</td>");
                tbody.Append("<td>" + string.Format("{0:N0}", transpotation.TotalPriceVND) + "</td>");
                tbody.AppendLine("</tr>");
                totalQuantity += transpotation.Quantity ?? 0;
                totalWeight += transpotation.Weight ?? 0;
                totalVolume += transpotation.Volume ?? 0;
            }

            tbody.Append($"<tr class=\"font-bold text-center\"> <td colspan=\"11\">VOUCHER CỦA QUÝ KHÁCH ĐƯỢC GIẢM</td> <td>{string.Format("{0:N0}", totalDecreaseAmount)} VND</td> </tr>");
            tbody.Append($"<tr class=\"font-bold text-center\"> <td colspan=\"4\">TỔNG SỐ KIỆN</td><td colspan=\"4\">TỔNG CÂN NẶNG</td><td colspan=\"4\">TỔNG SỐ KHỐI</td></tr>");
            tbody.Append($"<tr class=\"font-bold text-center\"> <td colspan=\"4\">{totalQuantity} KIỆN</td>  <td colspan=\"4\">{Math.Round(totalWeight, 1)} KG</td> <td colspan=\"4\">{Math.Round(totalVolume, 5)} M3</td> </tr>");
            tbody.Append($"<tr class=\"font-bold text-center\"> <td colspan=\"11\">TỔNG TIỀN</td> <td>{string.Format("{0:N0}", outOfStocks.Sum(x => x.TotalPriceVND))} VND</td> </tr>");

            string filePath = Path.Combine(_env.WebRootPath, "templates", "OutStockTemplate.html");
            string htmlContent = File.ReadAllText(filePath);
            // Dữ liệu động
            string pxkName = $"{string.Join(" ", ids)}";
            string dateExport = (outOfStock.DateOutStock ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm");
            string username = customer.Username;
            string phone = customer.Phone;
            string postOffice = outOfStock.PostOffice;
            string userAddress = customer.Address;
            string fullname = customer.FullName;

            var amount = outOfStocks.Sum(x => x.TotalPriceVND).ToString();

            // encode để tránh lỗi URL
            var addInfoRaw = $"{pxkName} {username}";
            var addInfo = Uri.EscapeDataString(addInfoRaw);

            // lấy base64 QR
            var qrBase64 = await GetVietQrBase64(amount, addInfo);
            // Thay thế các placeholder bằng dữ liệu thực tế
            htmlContent = htmlContent
                        .Replace("{id}", pxkName)
                        .Replace("{dateExport}", dateExport)
                        .Replace("{username}", username)
                        .Replace("{phone}", phone)
                        .Replace("{postOffice}", postOffice)
                        .Replace("{userAddress}", userAddress)
                        .Replace("{htmlPdf}", tbody.ToString())
                        .Replace("{TotalPriceVND}", amount)
                        .Replace("{fullname}", fullname)
                        .Replace("{qrBase64}", qrBase64);
            return htmlContent;
        }

        private async Task<string> GetVietQrBase64(string amount, string addInfo)
        {
            var url = $"https://img.vietqr.io/image/TCB-843573184633-compact2.png?amount={amount}&addInfo={addInfo}&accountName=DO TU ANH";

            using var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(url);

            var base64 = Convert.ToBase64String(bytes);
            return $"data:image/png;base64,{base64}";
        }
    }
}
