using AutoMapper;
using Azure.Core;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;
using static WebMVC.Services.ZaloAPIService;
namespace WebMVC.Services
{
    public class TransportationService : ITransportationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;
        private readonly ISignalRService _signalRService;
        private readonly INotificationService _notificationService;
        private readonly IZaloAPIService _zaloAPIService;
        private readonly IUploadFileService _uploadFileService;

        public TransportationService(IUnitOfWork unitOfWork, IMapper mapper,
            IHttpContextService httpContextService, ISignalRService signalRService,
            INotificationService notificationService, IZaloAPIService zaloAPIService,
            IUploadFileService uploadFileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextService = httpContextService;
            _signalRService = signalRService;
            _notificationService = notificationService;
            _zaloAPIService = zaloAPIService;
            _uploadFileService = uploadFileService;
        }
        public async Task<bool> AssignTransportation(AssignTransportationRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var currentAccount = await _httpContextService.GetCurrentAccount();
                var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode);
                var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Username == request.AccountName);
                if (customerAccount == null)
                {
                    throw new AppException("Không tìm thấy tài khoản");
                }
                transportation.FromId = request.FromId;
                transportation.ToId = request.ToId;
                transportation.ShipId = request.ShipId;
                transportation.AccountId = customerAccount.Id;
                transportation.StaffId = customerAccount.SaleId;
                transportation.PostOffice = currentAccount.PostOffice;
                _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<TransportationHistory>().Add(new TransportationHistory
                {
                    TransportationId = transportation.Id,
                    Content = string.Format(HistoryContent.GAN_DON, currentAccount.Username, transportation.Barcode, customerAccount.Username)
                }, currentDate, currentAccount.Id);
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
        public async Task<bool> Cancel(int id)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            return await CancelForAPI(id, currentAccount);
        }
        public async Task<bool> CancelForAPI(int id, Account currentAccount)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
                //transportation.Status = (int)ETransportationStatus.Cancel;
                //transportation = await ChangeStatusDateAsync(transportation, currentDate, currentAccount.Id);

                //_unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
                _unitOfWork.Repository<Transportation>().Delete(transportation);
                await _unitOfWork.Repository<TransportationHistory>().Add(new TransportationHistory
                {
                    TransportationId = id,
                    Content = string.Format(HistoryContent.HUY_DON, currentAccount.Username, transportation.Barcode)
                }, currentDate, currentAccount.Id);

                var voucherAccount = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().FirstOrDefaultAsync(x => x.Id == transportation.VoucherId);
                if (voucherAccount != null)
                {
                    voucherAccount.Status = (int)EVoucherAccountStatus.New;
                    _unitOfWork.Repository<VoucherAccount>().Update(voucherAccount, currentDate, currentAccount.Id);
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

        public async Task<bool> Create(CreateTransportationRequest request)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            return await CreateForApi(request, currentAccount);
        }

        public async Task<bool> CreateForApi(CreateTransportationRequest request, Account currentAccount)
        {
            try
            {
                var warehouseFrom = await _unitOfWork.Repository<Warehouse>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.WarehouseFrom && x.Type == (int)EWarehouseType.Reciever && x.Status == (int)EWarehouseStatus.Active);
                var warehouseTo = await _unitOfWork.Repository<Warehouse>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.WarehouseTo && x.Type == (int)EWarehouseType.Destination && x.Status == (int)EWarehouseStatus.Active);
                var ship = await _unitOfWork.Repository<Warehouse>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.TransportMethod && x.Type == (int)EWarehouseType.Shipping && x.Status == (int)EWarehouseStatus.Active);
                //var ship = await _unitOfWork.Repository<Warehouse>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.TransportMethod && x.Type == (int)EWarehouseType.Shipping && (x.Status == (int)EWarehouseStatus.Active || x.Status == (int)EWarehouseStatus.Special));
                var config = await _unitOfWork.Repository<WebConfiguration>().GetQueryable().SingleOrDefaultAsync();
                if (warehouseFrom == null || warehouseTo == null || ship == null || config == null)
                {
                    throw new AppException($"Lỗi không tìm thấy thông tin vận chuyển");
                }
                decimal currency = config.Currency;
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var transportations = new List<Transportation>();
                var transportationProducts = new List<TransportationProduct>();

                foreach (var item in request.Items)
                {
                    var voucherAccount = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.VoucherId
                        && x.Status == (int)EVoucherAccountStatus.New
                        && x.AccountId == currentAccount.Id
                        && x.StartDate.Date <= currentDate.Date && x.EndDate.Date >= currentDate.Date
                    );
                    if (transportations.Any(t => t.Barcode == item.Barcode))
                    {
                        throw new AppException($"Lỗi đã tồn tại mã {item.Barcode}");
                    }
                    if (voucherAccount != null)
                    {
                        var voucher = await _unitOfWork.Repository<Voucher>().GetQueryable().FirstOrDefaultAsync(x => x.Id == voucherAccount.VoucherId && x.Status == (int)EVoucherStatus.Active);
                        if (voucher == null)
                            throw new AppException($"Lỗi sử dụng voucher của đơn {item.Barcode}");
                        voucherAccount.Status = (int)EVoucherAccountStatus.Used;
                        _unitOfWork.Repository<VoucherAccount>().Update(voucherAccount, currentDate, currentAccount.Id);
                    }
                    var image = item.ImageUrl;
                    if (item.Images != null)
                    {
                        var listImageUrl = new List<string>();
                        foreach (var itemImage in item.Images)
                        {
                            listImageUrl.Add(await _uploadFileService.UploadFile(itemImage));
                        }
                        image = string.Join('|', listImageUrl);
                    }
                    var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().FirstOrDefaultAsync(x => x.Barcode == item.Barcode.Trim());
                    if (transportation != null)
                    {
                        if (transportation.AccountId > 0)
                            throw new AppException($"Đơn hàng #{item.Barcode} đã tồn tại");
                        transportation.AccountId = currentAccount.Id;
                        transportation.UserNote = item.Note;
                        // transportation.hscode = item.hscode;
                        transportation.FromId = request.WarehouseFrom;
                        transportation.ToId = request.WarehouseTo;
                        transportation.ShipId = request.TransportMethod;
                        transportation.VoucherId = voucherAccount?.Id;
                        transportation.VoucherInfo = $"{voucherAccount?.Name} \n {string.Format("{0:N0}", voucherAccount?.Amount)} {(voucherAccount == null ? "" : "đ")}";
                        transportation.Discount = voucherAccount?.Amount ?? 0;
                        transportation.StaffId = currentAccount.SaleId;
                        transportation.PostOffice = currentAccount.PostOffice;

                        transportation.UserUploadWeight = item.UserUploadWeight;
                        transportation.UserUploadVolume = item.UserUploadVolume;
                        transportation.UserUploadQuantity = item.UserUploadQuantity;
                        transportation.UserUploadImage = image;

                        _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);

                        await _unitOfWork.Repository<TransportationHistory>().Add(new TransportationHistory
                        {
                            TransportationId = transportation.Id,
                            Content = string.Format(HistoryContent.NHAN_DON, currentAccount.Username, item.Barcode)
                        }, currentDate, currentAccount.Id);
                    }
                    else
                    {
                        transportation = new Transportation
                        {
                            Barcode = item.Barcode,
                            // hscode = item.hscode,
                            UserNote = item.Note,
                            Status = (int)ETransportationStatus.New,
                            Weight = 0,
                            Volume = 0,
                            Quantity = 1,
                            Currency = currency,
                            Surcharge = 0,
                            PriceShipping = 0,
                            TotalPriceVND = 0,
                            Discount = voucherAccount?.Amount ?? 0,

                            AccountId = currentAccount.Id,
                            FromId = request.WarehouseFrom,
                            ToId = request.WarehouseTo,
                            ShipId = request.TransportMethod,
                            VoucherId = voucherAccount?.Id,
                            VoucherInfo = $"{voucherAccount?.Name} \n {string.Format("{0:N0}", voucherAccount?.Amount)} đ",
                            StaffId = currentAccount.SaleId,

                            PostOffice = currentAccount.PostOffice,

                            UserUploadWeight = item.UserUploadWeight,
                            UserUploadVolume = item.UserUploadVolume,
                            UserUploadQuantity = item.UserUploadQuantity,
                            UserUploadImage = image

                        };
                        transportations.Add(transportation);
                        await _unitOfWork.Repository<Transportation>().Add(transportation, currentDate, currentAccount.Id);
                        await _unitOfWork.SaveAsync();
                    }

                    if (item.Products != null)
                    {
                        foreach (var product in item.Products)
                        {
                            var productImage = product.ImageUrl;
                            if (product.Image != null)
                                productImage = await _uploadFileService.UploadFile(product.Image);
                            transportationProducts.Add(new TransportationProduct
                            {
                                TransportationId = transportation.Id,
                                Name = product.Name,
                                Quantity = product.Quantity,
                                Dimensions = product.Dimensions,
                                OtherInfor = product.OtherInfor,
                                Image = productImage,
                            });
                        }
                    }
                }
                await _unitOfWork.Repository<TransportationProduct>().AddRange(transportationProducts, currentDate, currentAccount.Id);

                var histories = new List<TransportationHistory>();
                foreach (var transportation in transportations)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.TAO_DON, currentAccount.Username, transportation.Barcode)
                    });
                }
                await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();

                var notification = new Notification
                {
                    Title = "Đơn hàng mới",
                    Content = $"Khách hàng {currentAccount.Username} đã lên đơn hàng mới",
                    WebUrl = "/transportation",
                    Type = (int)ENotificationType.Order,
                    IsStaff = true,
                };
                await _notificationService.SendNotification(notification, currentDate, currentAccount.Id,
                    staffId: currentAccount.SaleId ?? 0, staffRoleIds: new List<int> { (int)ERoleId.Admin, (int)ERoleId.Manager, });
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                var error = ex.ToString();
                Console.WriteLine(ex.ToString());
                throw new AppException(error);
            }
        }
        public async Task<bool> CreateFloatingTransportation(string barcode, int type, bool isCheckExist = true, int? bigPackageId = null)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().FirstOrDefaultAsync(x => x.Barcode == barcode.Trim());
            if (transportation != null)
            {
                if (isCheckExist)
                {
                    throw new AppException($"Đơn hàng {barcode} đã tồn tại");
                }
                else
                {
                    return true;
                }
            }
            return await CreateFloatingTransportationForApi(barcode, type, currentDate, currentAccount);
        }
        public async Task<bool> CreateFloatingTransportationForApi(string barcode, int type, DateTime currentDate, Account currentAccount, int? bigPackageId = null)
        {
            try
            {
                var config = await _unitOfWork.Repository<WebConfiguration>().GetQueryable().SingleOrDefaultAsync();
                decimal currency = config.Currency;
                await _unitOfWork.BeginTransactionAsync();

                int shipId = 4;
                var listAccountCN = new List<int>() { 20401, 20400 };
                var listAccountHT = new List<int>() { 15780, 15338 };
                var listAccountDT = new List<int>() { 20398 };
                if (listAccountCN.Contains(currentAccount.Id))
                {
                    shipId = 5;
                }
                else if (listAccountHT.Contains(currentAccount.Id))
                {
                    shipId = 4;
                }
                else if (listAccountDT.Contains(currentAccount.Id))
                {
                    shipId = 3;
                }
                var transportation = new Transportation
                {
                    Barcode = barcode,
                    UserNote = "",
                    Status = (int)ETransportationStatus.New,
                    Type = type,
                    Weight = 0,
                    Volume = 0,
                    Quantity = 1,
                    Currency = currency,
                    Surcharge = 0,
                    PriceShipping = 0,
                    TotalPriceVND = 0,
                    PostOffice = currentAccount.PostOffice ?? PostOfficeName.GetPostOffice().FirstOrDefault(),

                    FromId = 0,
                    ToId = 0,
                    ShipId = shipId,

                    BigPackageId = bigPackageId
                };
                await _unitOfWork.Repository<Transportation>().Add(transportation, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();

                await _unitOfWork.Repository<TransportationHistory>().Add(new TransportationHistory
                {
                    TransportationId = transportation.Id,
                    Content = string.Format(HistoryContent.TAO_DON, currentAccount.Username, transportation.Barcode)
                }, currentDate, currentAccount.Id);

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

        public async Task<List<TransportationResponse>> GetByBigPackageId(int bigPackageId)
        {
            var transportations = await (from transportation in _unitOfWork.Repository<Transportation>().GetQueryable()
                                         join account in _unitOfWork.Repository<Account>().GetQueryable() on transportation.AccountId equals account.Id into accountJoin
                                         from account in accountJoin.DefaultIfEmpty()
                                         where transportation.Status != (int)ETransportationStatus.Cancel
                                                && transportation.BigPackageId == bigPackageId
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
                                             AccountId = account.Id,
                                             AccountName = account != null ? account.Username : string.Empty,
                                             DateArrivedAtVNWarehouse = transportation.DateArrivedAtVNWarehouse,
                                             StaffNote = transportation.StaffNote
                                         })
                                          .OrderByDescending(x => x.AccountId)
                                          .ToListAsync();
            return transportations;
        }

        //public async Task<PagedList<TransportationResponse>> GetPaging(TransportationSearch search)
        //{
        //    var loggedModel = _httpContextService.GetLoggedModel();
        //    if (loggedModel.IsStaff == 0)
        //    {
        //        search.AccountId = loggedModel.Id;
        //    }
        //    var transportations = await (from transportation in _unitOfWork.Repository<Transportation>().GetQueryable()
        //                                 join account in _unitOfWork.Repository<Account>().GetQueryable() on transportation.AccountId equals account.Id into accountJoin
        //                                 from account in accountJoin.DefaultIfEmpty()
        //                                 join warehouseFrom in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Reciever) on transportation.FromId equals warehouseFrom.Id into warehouseFromJoin
        //                                 from warehouseFrom in warehouseFromJoin.DefaultIfEmpty()
        //                                 join warehouseTo in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Destination) on transportation.ToId equals warehouseTo.Id into warehouseToJoin
        //                                 from warehouseTo in warehouseToJoin.DefaultIfEmpty()
        //                                 join shippingType in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Shipping) on transportation.ShipId equals shippingType.Id into shippingTypeJoin
        //                                 from shippingType in shippingTypeJoin.DefaultIfEmpty()
        //                                 where (loggedModel.IsStaff == 1 || transportation.Status != (int)ETransportationStatus.Cancel)
        //                                        && (search.AccountId == null || transportation.AccountId == search.AccountId)
        //                                        && (search.BigPackageId == null || transportation.BigPackageId == search.BigPackageId)
        //                                        && (search.Barcode == null || transportation.Barcode.Contains(search.Barcode))
        //                                        && (search.Status == null || transportation.Status == search.Status)
        //                                        && (search.FromDate == null || transportation.Created >= search.FromDate)
        //                                        && (search.ToDate == null || transportation.Created <= search.ToDate)
        //                                        && (search.PostOffice == null || transportation.PostOffice == search.PostOffice)
        //                                        && (search.IsNotAccount != true || transportation.AccountId == null)
        //                                 select new TransportationResponse
        //                                 {
        //                                     Id = transportation.Id,
        //                                     Barcode = transportation.Barcode,
        //                                     UserNote = transportation.UserNote,
        //                                     Status = transportation.Status,
        //                                     Weight = transportation.Weight,
        //                                     Volume = transportation.Volume,
        //                                     Quantity = transportation.Quantity,
        //                                     Currency = transportation.Currency,
        //                                     UnitWeight = transportation.UnitWeight,
        //                                     UnitVolume = transportation.UnitVolume,
        //                                     Surcharge = transportation.Surcharge,
        //                                     PriceShipping = transportation.PriceShipping,
        //                                     TotalPriceVND = transportation.TotalPriceVND,
        //                                     Created = transportation.Created,
        //                                     PostOffice = transportation.PostOffice,
        //                                     AccountId = account.Id,
        //                                     AccountName = account != null ? account.Username : string.Empty,
        //                                     WarehouseFrom = warehouseFrom != null ? warehouseFrom.Name : string.Empty,
        //                                     WarehouseTo = warehouseTo != null ? warehouseTo.Name : string.Empty,
        //                                     ShipName = shippingType != null ? shippingType.Name : string.Empty
        //                                 })
        //                      .OrderByDescending(x => x.Id)
        //                      .Skip((search.PageIndex - 1) * search.PageSize)
        //                      .Take(search.PageSize)
        //                      .ToListAsync();

        //    int total = await _unitOfWork.Repository<Transportation>().GetQueryable()
        //                    .Where(x => (loggedModel.IsStaff == 1 || x.Status != (int)ETransportationStatus.Cancel)
        //                            && (search.AccountId == null || x.AccountId == search.AccountId)
        //                            && (search.BigPackageId == null || x.BigPackageId == search.BigPackageId)
        //                            && (search.Barcode == null || x.Barcode.Contains(search.Barcode))
        //                            && (search.Status == null || x.Status == search.Status)
        //                            && (search.FromDate == null || x.Created >= search.FromDate)
        //                            && (search.ToDate == null || x.Created <= search.ToDate)
        //                            && (search.PostOffice == null || x.PostOffice == search.PostOffice)
        //                            && (search.IsNotAccount != true || x.AccountId == null)
        //                            )
        //                    .CountAsync();
        //    return new PagedList<TransportationResponse>
        //    {
        //        PageIndex = search.PageIndex,
        //        PageSize = search.PageSize,
        //        TotalItem = total,
        //        Items = transportations
        //    };
        //}

        public async Task<PagedList<TransportationResponse>> GetPaging(TransportationSearch search)
        {
            var loggedModel = _httpContextService.GetLoggedModel();
            return await GetPagingForAPI(search, loggedModel);

        }

        public async Task<PagedList<TransportationResponse>> GetPagingForAPI(TransportationSearch search, LoggedModel loggedModel)
        {
            int? saleId = null;

            if (loggedModel.IsStaff == 0)
            {
                search.AccountId = loggedModel.Id;
            }

            if (loggedModel.RoleId == (int)ERoleId.Sale && loggedModel.IsStaff == 1)
            {
                saleId = loggedModel.Id;
            }

            string dateSearchField = "Created";
            switch (search.Status)
            {
                case (int)ETransportationStatus.ArrivedAtTQWarehouse:
                    dateSearchField = "DateArrivedAtTQWarehouse";
                    break;
                case (int)ETransportationStatus.ExitedFromTQWarehouse:
                    dateSearchField = "DateExitedFromTQWarehouse";
                    break;
                case (int)ETransportationStatus.CustomsInspectedGoods:
                    dateSearchField = "DateCustomsInspectedGoods";
                    break;
                case (int)ETransportationStatus.ReturningToVNWarehouse:
                    dateSearchField = "DateReturningToVNWarehouse";
                    break;
                case (int)ETransportationStatus.ArrivedAtVNWarehouse:
                    dateSearchField = "DateArrivedAtVNWarehouse";
                    break;
                case (int)ETransportationStatus.Completed:
                    dateSearchField = "DateCompleted";
                    break;
                default:
                    break;
            }

            if (search.FilterDay.HasValue)
            {
                var today = DateTime.Today.AddDays(1).AddTicks(-1);
                var fromDate = DateTime.Today.AddDays(-search.FilterDay.Value);

                search.FromDate = fromDate;
                search.ToDate = today;
            }

            var dbContext = _unitOfWork.GetDbContext();

            string sql = $@"
            SELECT 
                t.Id, t.Barcode, t.UserNote, t.Status, t.Weight, t.Volume, t.Quantity, t.ShipId,
                t.Currency, t.UnitWeight, t.UnitVolume, t.Surcharge, t.PriceShipping, t.TotalPriceVND, t.Image,
                t.DateArrivedAtTQWarehouse,
                t.DateExitedFromTQWarehouse,
                t.DateCustomsInspectedGoods,
                t.DateReturningToVNWarehouse,
                t.DateArrivedAtVNWarehouse,
                t.DateCompleted,
                t.AccountArrivedAtTQWarehouse,
                t.UserUploadImage,
                t.Created, t.PostOffice, a.Id AS AccountId, a.Username AS AccountName, 
                wf.Name AS WarehouseFrom, wt.Name AS WarehouseTo, st.Name AS ShipName,
                bp.Name AS BigPackageName, bp.Partner AS PartnerInfor,
                t.IsOutStock
            FROM Transportations t
            LEFT JOIN Accounts a ON t.AccountId = a.Id
            LEFT JOIN BigPackages bp ON t.BigPackageId = bp.Id
            LEFT JOIN Warehouses wf ON t.FromId = wf.Id AND wf.Type = @WarehouseFromType
            LEFT JOIN Warehouses wt ON t.ToId = wt.Id AND wt.Type = @WarehouseToType
            LEFT JOIN Warehouses st ON t.ShipId = st.Id AND st.Type = @ShippingType
            WHERE (@AccountId IS NULL OR t.AccountId = @AccountId)
              AND (@IsOutStock IS NULL OR t.IsOutStock = @IsOutStock)
              AND (@StaffId IS NULL OR (t.StaffId = @StaffId OR t.StaffId IS NULL))
              AND (@BigPackageId IS NULL OR t.BigPackageId = @BigPackageId)
              AND (@ShipId IS NULL OR t.ShipId = @ShipId)
              AND (@IsNotAccount IS NULL OR t.AccountId IS NULL)
              AND (@Barcode IS NULL OR t.Barcode LIKE '%' + @Barcode + '%')
              AND (@BigPackageName IS NULL OR bp.Name LIKE '%' + @BigPackageName + '%')
              AND (@Status IS NULL OR t.Status = @Status)
              AND (@Type IS NULL OR t.Type = @Type)
              AND (@FromDate IS NULL OR t.{dateSearchField} >= @FromDate)
              AND (@ToDate IS NULL OR t.{dateSearchField} <= @ToDate)
              AND (@PostOffice IS NULL OR t.PostOffice = @PostOffice) ";

            string sqlTotal = $@"
                SELECT COUNT(t.Id) FROM Transportations t
                LEFT JOIN BigPackages bp ON t.BigPackageId = bp.Id
                WHERE (@AccountId IS NULL OR t.AccountId = @AccountId)
                  AND (@IsOutStock IS NULL OR t.IsOutStock = @IsOutStock)
                  AND (@StaffId IS NULL OR (t.StaffId = @StaffId OR t.StaffId IS NULL))
                  AND (@BigPackageId IS NULL OR t.BigPackageId = @BigPackageId)
                  AND (@ShipId IS NULL OR t.ShipId = @ShipId)
                  AND (@IsNotAccount IS NULL OR t.AccountId IS NULL)
                  AND (@Barcode IS NULL OR t.Barcode LIKE '%' + @Barcode + '%')
                  AND (@BigPackageName IS NULL OR bp.Name LIKE '%' + @BigPackageName + '%')
                  AND (@Status IS NULL OR t.Status = @Status)
                  AND (@Type IS NULL OR t.Type = @Type)
                  AND (@FromDate IS NULL OR t.{dateSearchField} >= @FromDate)
                  AND (@ToDate IS NULL OR t.{dateSearchField} <= @ToDate)
                  AND (@PostOffice IS NULL OR t.PostOffice = @PostOffice)";

            if (loggedModel.IsStaff == 0)
            {
                sql += @$" AND t.Status != {(int)ETransportationStatus.Cancel}";
                sqlTotal += @$" AND t.Status != {(int)ETransportationStatus.Cancel}";
            }
            sql += @"
            ORDER BY t.Id DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AccountId", search.AccountId ?? (object)DBNull.Value),
                new SqlParameter("@IsOutStock", search.IsOutStock ?? (object)DBNull.Value),
                new SqlParameter("@StaffId", saleId ?? (object)DBNull.Value),
                new SqlParameter("@BigPackageId", search.BigPackageId ?? (object)DBNull.Value),
                new SqlParameter("@ShipId", search.ShipId ?? (object)DBNull.Value),
                new SqlParameter("@IsNotAccount", search.IsNotAccount ?? (object)DBNull.Value),
                new SqlParameter("@Barcode", search.Barcode ?? (object)DBNull.Value),
                new SqlParameter("@BigPackageName", search.BigPackageName ?? (object)DBNull.Value),
                new SqlParameter("@Status", search.Status ?? (object)DBNull.Value),
                new SqlParameter("@Type", search.Type ?? (object)DBNull.Value),
                new SqlParameter("@FromDate", search.FromDate ?? (object)DBNull.Value),
                new SqlParameter("@ToDate", search.ToDate ?? (object)DBNull.Value),
                new SqlParameter("@PostOffice", search.PostOffice ?? (object)DBNull.Value),
                new SqlParameter("@Offset", (search.PageIndex - 1) * search.PageSize),
                new SqlParameter("@PageSize", search.PageSize),
                new SqlParameter("@WarehouseFromType", (int)EWarehouseType.Reciever),
                new SqlParameter("@WarehouseToType", (int)EWarehouseType.Destination),
                new SqlParameter("@ShippingType", (int)EWarehouseType.Shipping)
            };
            string a = GetFinalSqlQuery(sql, parameters);
            using (var connection = dbContext.Database.GetDbConnection())
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddRange(parameters.ToArray());

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var transportations = new List<TransportationResponse>();
                            while (await reader.ReadAsync())
                            {
                                transportations.Add(new TransportationResponse
                                {
                                    Id = (int)reader["Id"],
                                    Barcode = reader["Barcode"].ToString(),
                                    UserNote = reader["UserNote"].ToString(),
                                    Status = (int)reader["Status"],
                                    Weight = reader["Weight"] as double?,
                                    Volume = reader["Volume"] as double?,
                                    Quantity = reader["Quantity"] as int?,
                                    ShipId = reader["ShipId"] as int?,
                                    Currency = (decimal)reader["Currency"],
                                    UnitWeight = reader["UnitWeight"] as decimal?,
                                    UnitVolume = reader["UnitVolume"] as decimal?,
                                    Surcharge = reader["Surcharge"] as decimal?,
                                    PriceShipping = reader["PriceShipping"] as decimal?,
                                    TotalPriceVND = (decimal)reader["TotalPriceVND"],
                                    Created = (DateTime)reader["Created"],
                                    DateArrivedAtTQWarehouse = reader["DateArrivedAtTQWarehouse"] != DBNull.Value
    ? (DateTime?)reader["DateArrivedAtTQWarehouse"]
    : null,

                                    DateExitedFromTQWarehouse = reader["DateExitedFromTQWarehouse"] != DBNull.Value
    ? (DateTime?)reader["DateExitedFromTQWarehouse"]
    : null,

                                    DateCustomsInspectedGoods = reader["DateCustomsInspectedGoods"] != DBNull.Value
    ? (DateTime?)reader["DateCustomsInspectedGoods"]
    : null,

                                    DateReturningToVNWarehouse = reader["DateReturningToVNWarehouse"] != DBNull.Value
    ? (DateTime?)reader["DateReturningToVNWarehouse"]
    : null,

                                    DateArrivedAtVNWarehouse = reader["DateArrivedAtVNWarehouse"] != DBNull.Value
    ? (DateTime?)reader["DateArrivedAtVNWarehouse"]
    : null,

                                    DateCompleted = reader["DateCompleted"] != DBNull.Value
    ? (DateTime?)reader["DateCompleted"]
    : null,
                                    AccountArrivedAtTQWarehouse = reader["AccountArrivedAtTQWarehouse"].ToString(),
                                    PostOffice = reader["PostOffice"].ToString(),
                                    AccountId = reader["AccountId"] as int?,
                                    AccountName = reader["AccountName"]?.ToString(),
                                    WarehouseFrom = reader["WarehouseFrom"]?.ToString(),
                                    WarehouseTo = reader["WarehouseTo"]?.ToString(),
                                    ShipName = reader["ShipName"]?.ToString(),
                                    BigPackageName = reader["BigPackageName"]?.ToString(),
                                    PartnerInfor = reader["PartnerInfor"]?.ToString(),
                                    Image = reader["Image"]?.ToString(),
                                    UserUploadImage = reader["UserUploadImage"]?.ToString(),
                                    IsOutStock = reader["IsOutStock"] as bool?,
                                });
                            }
                            var transportationIds = transportations.Where(x => x.ShipId == 5).Select(x => x.Id).ToList();
                            var products = await _unitOfWork
                                .Repository<TransportationProduct>()
                                .GetQueryable()
                                .Where(p => transportationIds.Contains(p.TransportationId))
                                .ToListAsync();

                            var productDict = products
                                .GroupBy(p => p.TransportationId)
                                .ToDictionary(g => g.Key, g => g.First());

                            foreach (var transportation in transportations)
                            {
                                if (productDict.TryGetValue(transportation.Id, out var product))
                                {
                                    transportation.UserUploadImage = product.Image;
                                }
                            }

                            var parametersTotal = parameters.Select(p => new SqlParameter(p.ParameterName, p.Value)).ToList();
                            string att = GetFinalSqlQuery(sqlTotal, parametersTotal);

                            using (var commandTotal = connection.CreateCommand())
                            {
                                commandTotal.CommandText = sqlTotal;
                                commandTotal.Parameters.AddRange(parametersTotal.ToArray());

                                int total = (int)await commandTotal.ExecuteScalarAsync();
                                return new PagedList<TransportationResponse>
                                {
                                    PageIndex = search.PageIndex,
                                    PageSize = search.PageSize,
                                    TotalItem = total,
                                    Items = transportations
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new AppException(ex.Message);
                    }
                }
            }
        }
        public async Task<TransportationResponse> UpdateBarcode(UpdateBarcodeRequest request, int? type = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var currentAccount = await _httpContextService.GetCurrentAccount();
                var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().FirstOrDefaultAsync(x => x.Barcode == request.Barcode.Trim() || x.Id == (request.Id ?? 0));
                if (transportation == null)
                {
                    throw new AppException($"Đơn hàng {request.Barcode} không tồn tại");
                }
                if (transportation.Status == (int)ETransportationStatus.Completed)
                {
                    throw new AppException($"Đơn hàng {request.Barcode} đã hoàn thành");
                }
                var tranportationOutOfStock = await _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable().FirstOrDefaultAsync(x => x.TransportationId == transportation.Id);
                if (tranportationOutOfStock != null)
                {
                    var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().FirstOrDefaultAsync(x => x.Id == tranportationOutOfStock.OutOfStockId && x.Status != (int)EOutOfStockStatus.Cancel);
                    if (outOfStock != null)
                    {
                        throw new AppException($"Đơn hàng {request.Barcode} đã nằm ở phiếu xuất kho #{outOfStock.Id}");
                    }
                }

                var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.AccountId);
                if (customerAccount?.MinWeight != null)
                {
                    if (request.Weight < customerAccount.MinWeight)
                    {
                        request.Weight = customerAccount.MinWeight ?? request.Weight;
                    }
                }
                else
                {
                    if (request.Weight < 1)
                    {
                        request.Weight = 1;
                    }
                }
                var histories = new List<TransportationHistory>();

                if (transportation.Barcode != request.Barcode)
                {
                    transportation.Barcode = request.Barcode;

                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_MVD, currentAccount.Username, transportation.Barcode, transportation.Barcode, request.Barcode)
                    });
                }
                if (transportation.Weight != request.Weight)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_CAN_NANG_DON, currentAccount.Username, transportation.Barcode, transportation.Weight ?? 0, request.Weight)
                    });
                }
                if (transportation.Volume != request.Volume)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_SO_KHOI_DON, currentAccount.Username, transportation.Barcode, transportation.Volume ?? 0, request.Volume)
                    });
                }
                if (transportation.Quantity != request.Quantity)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_SO_KIEN_DON, currentAccount.Username, transportation.Barcode, transportation.Quantity ?? 0, request.Quantity)
                    });
                }
                if (transportation.Surcharge != request.Surcharge)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_PHU_PHI_DON, currentAccount.Username, transportation.Barcode, string.Format("{0:N2}", transportation.Surcharge), string.Format("{0:N2}", request.Surcharge))
                    });
                }
                int oldStatus = transportation.Status;
                int newStatus = request.Status ?? oldStatus;
                histories.Add(new TransportationHistory
                {
                    TransportationId = transportation.Id,
                    Content = string.Format(HistoryContent.DOI_TRANG_THAI_DON, currentAccount.Username, transportation.Barcode, ETransportationStatusName.GetStatusName(oldStatus), ETransportationStatusName.GetStatusName(newStatus))
                });
                if (type != null)
                {
                    transportation.Type = type;
                }
                transportation.Status = newStatus;
                transportation.Quantity = request.Quantity;
                transportation.Weight = request.Weight;
                transportation.Volume = request.Volume;
                transportation.Surcharge = request.Surcharge;
                transportation.StaffNote = request.StaffNote;
                decimal surchargeVND = Math.Round(request.Surcharge * transportation.Currency, 0);
                if (transportation.Status == (int)ETransportationStatus.ArrivedAtVNWarehouse)
                {
                    var priceSeparateList = await _unitOfWork.Repository<PricingSeparate>().GetQueryable().Where(x => x.ShipId == transportation.ShipId && x.AccountId == transportation.AccountId).ToListAsync();
                    var priceList = await _unitOfWork.Repository<Pricing>().GetQueryable().Where(x => x.FromWarehouseId == transportation.FromId && x.ToWarehouseId == transportation.ToId && x.ShipId == transportation.ShipId).ToListAsync();
                    if (!(transportation.UnitWeight > 0) && transportation.Weight == request.Weight)
                    {
                        var weightSeparatePricing = priceSeparateList.Where(x => x.Type == (int)EPricingType.Weight && transportation.Weight >= x.RangeMin && transportation.Weight < x.RangeMax).FirstOrDefault();
                        if (weightSeparatePricing != null)
                        {
                            transportation.UnitWeight = weightSeparatePricing.PricePerUnit;
                        }
                        else
                        {
                            var weightPricing = priceList.Where(x => x.Type == (int)EPricingType.Weight && transportation.Weight >= x.RangeMin && transportation.Weight < x.RangeMax).FirstOrDefault();
                            if (weightPricing == null)
                            {
                                transportation.UnitWeight = 0;
                            }
                            else
                            {
                                transportation.UnitWeight = weightPricing.PricePerUnit;
                            }
                        }
                    }
                    if (!(transportation.UnitVolume > 0) && transportation.Volume == request.Volume)
                    {
                        var volumeSeparatePricing = priceSeparateList.Where(x => x.Type == (int)EPricingType.Volume && transportation.Volume >= x.RangeMin && transportation.Volume < x.RangeMax).FirstOrDefault();
                        if (volumeSeparatePricing != null)
                        {
                            transportation.UnitWeight = volumeSeparatePricing.PricePerUnit;
                        }
                        else
                        {
                            var volumePricing = priceList.Where(x => x.Type == (int)EPricingType.Volume && transportation.Volume >= x.RangeMin && transportation.Volume < x.RangeMax).FirstOrDefault();
                            if (volumePricing == null)
                            {
                                transportation.UnitVolume = 0;
                            }
                            else
                            {
                                transportation.UnitVolume = volumePricing.PricePerUnit;
                            }
                        }
                    }
                    decimal? priceWeight = transportation.UnitWeight * (decimal)transportation.Weight;
                    decimal? priceVolume = transportation.UnitVolume * (decimal)transportation.Volume;
                    transportation.PriceShipping = priceWeight > priceVolume ? priceWeight : priceVolume;

                    var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().FirstOrDefaultAsync(x => x.Id == transportation.BigPackageId);
                    if (bigPackage != null)
                    {
                        if (bigPackage.Status != (int)EBigPackageStatus.ArrivedAtVNWarehouse)
                        {
                            await _unitOfWork.Repository<BigPackageHistory>().Add(new BigPackageHistory
                            {
                                BigPackageId = bigPackage.Id,
                                Content = string.Format(HistoryContent.DOI_TRANG_THAI_BAO, currentAccount.Username, bigPackage.Name, EBigPackageStatusName.GetStatusName(bigPackage.Status), EBigPackageStatusName.GetStatusName((int)EBigPackageStatus.ArrivedAtVNWarehouse)),
                            }, currentDate, currentAccount.Id);
                        }
                        bigPackage.Status = (int)EBigPackageStatus.ArrivedAtVNWarehouse;
                        _unitOfWork.Repository<BigPackage>().Update(bigPackage, currentDate, currentAccount.Id);
                    }
                }
                transportation.TotalPriceVND = (transportation.PriceShipping ?? 0) + surchargeVND;

                var voucherAccount = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.VoucherId && x.Status == (int)EVoucherAccountStatus.Used);
                transportation.Discount = voucherAccount?.Amount ?? 0;
                if (transportation.Discount > transportation.TotalPriceVND)
                {
                    transportation.Discount = transportation.TotalPriceVND;
                }
                transportation.TotalPriceVND -= transportation.Discount;
                transportation = await ChangeStatusDateAsync(transportation, currentAccount.Username, currentDate, currentAccount.Id);

                if (!string.IsNullOrEmpty(request.Image))
                {
                    transportation.Image = request.Image;
                }
                _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                await _signalRService.SendConfirmNotification($"UpdatedBarcode-{transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");

                return _mapper.Map<TransportationResponse>(transportation);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<bool> UpdateBarcodeMultiple(List<UpdateBarcodeRequest> requests, int status, int type)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            return await UpdateBarcodeMultipleForApi(requests, status, type, currentDate, currentAccount);
        }

        public async Task<bool> UpdateBarcodeMultipleForApi(List<UpdateBarcodeRequest> requests, int status, int type, DateTime currentDate, Account currentAccount)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var transportations = new List<Transportation>();
                var histories = new List<TransportationHistory>();
                foreach (var request in requests)
                {
                    var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().FirstOrDefaultAsync(x => x.Barcode == request.Barcode || x.Id == request.Id);
                    if (transportation == null)
                    {
                        continue;
                    }
                    if (transportation.Status == (int)ETransportationStatus.Completed)
                    {
                        continue;
                    }
                    var tranportationOutOfStock = await _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable().FirstOrDefaultAsync(x => x.TransportationId == transportation.Id);
                    if (tranportationOutOfStock != null)
                    {
                        var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().FirstOrDefaultAsync(x => x.Id == tranportationOutOfStock.OutOfStockId && x.Status != (int)EOutOfStockStatus.Cancel);
                        if (outOfStock != null)
                        {
                            continue;
                        }
                    }
                    var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.AccountId);
                    if (customerAccount?.MinWeight != null)
                    {
                        if (request.Weight < customerAccount.MinWeight)
                        {
                            request.Weight = customerAccount.MinWeight ?? request.Weight;
                        }
                    }
                    else
                    {
                        if (request.Weight < 1)
                        {
                            request.Weight = 1;
                        }
                    }
                    if (transportation.Weight != request.Weight)
                    {
                        histories.Add(new TransportationHistory
                        {
                            TransportationId = transportation.Id,
                            Content = string.Format(HistoryContent.DOI_CAN_NANG_DON, currentAccount.Username, transportation.Barcode, transportation.Weight ?? 0, request.Weight)
                        });
                    }
                    if (transportation.Volume != request.Volume)
                    {
                        histories.Add(new TransportationHistory
                        {
                            TransportationId = transportation.Id,
                            Content = string.Format(HistoryContent.DOI_SO_KHOI_DON, currentAccount.Username, transportation.Barcode, transportation.Volume ?? 0, request.Volume)
                        });
                    }
                    if (transportation.Quantity != request.Quantity)
                    {
                        histories.Add(new TransportationHistory
                        {
                            TransportationId = transportation.Id,
                            Content = string.Format(HistoryContent.DOI_SO_KIEN_DON, currentAccount.Username, transportation.Barcode, transportation.Quantity ?? 0, request.Quantity)
                        });
                    }
                    if (transportation.Surcharge != request.Surcharge)
                    {
                        histories.Add(new TransportationHistory
                        {
                            TransportationId = transportation.Id,
                            Content = string.Format(HistoryContent.DOI_PHU_PHI_DON, currentAccount.Username, transportation.Barcode, string.Format("{0:N2}", transportation.Surcharge), string.Format("{0:N2}", request.Surcharge))
                        });
                    }

                    var listSpecialAccountShipId = new List<int> { 20401, 20400, 15780, 15338, 20398 };
                    if (transportation.ShipId != request.ShipId && request.ShipId != null && !listSpecialAccountShipId.Contains(currentAccount.Id))
                    {
                        histories.Add(new TransportationHistory
                        {
                            TransportationId = transportation.Id,
                            Content = string.Format(HistoryContent.DOI_PTVC_DON, currentAccount.Username, transportation.Barcode, transportation.ShipId ?? 0, request.ShipId)
                        });
                        transportation.ShipId = request.ShipId;
                    }
                    int oldStatus = transportation.Status;
                    int oldType = transportation.Type ?? (int)ETransportationType.HangLe1;
                    int newStatus = status;
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_TRANG_THAI_DON, currentAccount.Username, transportation.Barcode, ETransportationStatusName.GetStatusName(oldStatus), ETransportationStatusName.GetStatusName(newStatus))
                    });
                    transportation.Type = type;
                    transportation.Status = newStatus;
                    transportation.Quantity = request.Quantity;
                    transportation.Weight = request.Weight;
                    transportation.Volume = request.Volume;
                    transportation.Surcharge = request.Surcharge;
                    transportation.StaffNote = request.StaffNote;
                    decimal surchargeVND = Math.Round(request.Surcharge * transportation.Currency, 0);
                    if (transportation.Status == (int)ETransportationStatus.ArrivedAtVNWarehouse)
                    {
                        transportation.Type = oldType;
                        var priceSeparateList = await _unitOfWork.Repository<PricingSeparate>().GetQueryable().Where(x => x.ShipId == transportation.ShipId && x.AccountId == transportation.AccountId).ToListAsync();
                        var priceList = await _unitOfWork.Repository<Pricing>().GetQueryable().Where(x => x.FromWarehouseId == transportation.FromId && x.ToWarehouseId == transportation.ToId && x.ShipId == transportation.ShipId).ToListAsync();
                        if (!(transportation.UnitWeight > 0) && transportation.Weight == request.Weight)
                        {
                            var weightSeparatePricing = priceSeparateList.Where(x => x.Type == (int)EPricingType.Weight && transportation.Weight >= x.RangeMin && transportation.Weight < x.RangeMax).FirstOrDefault();
                            if (weightSeparatePricing != null)
                            {
                                transportation.UnitWeight = weightSeparatePricing.PricePerUnit;
                            }
                            else
                            {
                                var weightPricing = priceList.Where(x => x.Type == (int)EPricingType.Weight && transportation.Weight >= x.RangeMin && transportation.Weight < x.RangeMax).FirstOrDefault();
                                if (weightPricing == null)
                                {
                                    transportation.UnitWeight = 0;
                                }
                                else
                                {
                                    transportation.UnitWeight = weightPricing.PricePerUnit;
                                }
                            }
                        }
                        if (!(transportation.UnitVolume > 0) && transportation.Volume == request.Volume)
                        {
                            var volumeSeparatePricing = priceSeparateList.Where(x => x.Type == (int)EPricingType.Volume && transportation.Volume >= x.RangeMin && transportation.Volume < x.RangeMax).FirstOrDefault();
                            if (volumeSeparatePricing != null)
                            {
                                transportation.UnitWeight = volumeSeparatePricing.PricePerUnit;
                            }
                            else
                            {
                                var volumePricing = priceList.Where(x => x.Type == (int)EPricingType.Volume && transportation.Volume >= x.RangeMin && transportation.Volume < x.RangeMax).FirstOrDefault();
                                if (volumePricing == null)
                                {
                                    transportation.UnitVolume = 0;
                                }
                                else
                                {
                                    transportation.UnitVolume = volumePricing.PricePerUnit;
                                }
                            }
                        }
                        decimal? priceWeight = transportation.UnitWeight * (decimal)transportation.Weight;
                        decimal? priceVolume = transportation.UnitVolume * (decimal)transportation.Volume;
                        transportation.PriceShipping = priceWeight > priceVolume ? priceWeight : priceVolume;

                        var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().FirstOrDefaultAsync(x => x.Id == transportation.BigPackageId);
                        if (bigPackage != null)
                        {
                            if (bigPackage.Status != (int)EBigPackageStatus.ArrivedAtVNWarehouse)
                            {
                                await _unitOfWork.Repository<BigPackageHistory>().Add(new BigPackageHistory
                                {
                                    BigPackageId = bigPackage.Id,
                                    Content = string.Format(HistoryContent.DOI_TRANG_THAI_BAO, currentAccount.Username, bigPackage.Name, EBigPackageStatusName.GetStatusName(bigPackage.Status), EBigPackageStatusName.GetStatusName((int)EBigPackageStatus.ArrivedAtVNWarehouse)),

                                }, currentDate, currentAccount.Id);
                            }
                            bigPackage.Status = (int)EBigPackageStatus.ArrivedAtVNWarehouse;
                            _unitOfWork.Repository<BigPackage>().Update(bigPackage, currentDate, currentAccount.Id);
                        }
                    }
                    transportation.TotalPriceVND = (transportation.PriceShipping ?? 0) + surchargeVND;
                    var voucherAccount = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.VoucherId && x.Status == (int)EVoucherAccountStatus.Used);
                    transportation.Discount = voucherAccount?.Amount ?? 0;
                    if (transportation.Discount > transportation.TotalPriceVND)
                    {
                        transportation.Discount = transportation.TotalPriceVND;
                    }
                    transportation.TotalPriceVND -= transportation.Discount;
                    transportation = await ChangeStatusDateAsync(transportation, currentAccount.Username, currentDate, currentAccount.Id);
                    transportation.Image = request.Image;
                    transportations.Add(transportation);
                }
                _unitOfWork.Repository<Transportation>().UpdateRange(transportations, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                foreach (var request in requests)
                {
                    await _signalRService.SendConfirmNotification($"UpdatedBarcode-${request.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");
                }
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<ScanResponse> ScanByBarcode(string barcode, int? type)
        {
            var scanResponse = await (from transportation in _unitOfWork.Repository<Transportation>().GetQueryable()
                                      join account in _unitOfWork.Repository<Account>().GetQueryable() on transportation.AccountId equals account.Id into accountJoin
                                      from account in accountJoin.DefaultIfEmpty()
                                      where transportation.Barcode == barcode.Trim()
                                      select new ScanResponse
                                      {
                                          Id = transportation.Id,
                                          Barcode = transportation.Barcode,
                                          Quantity = transportation.Quantity ?? 0,
                                          Weight = transportation.Weight ?? 0,
                                          Volume = transportation.Volume ?? 0,
                                          Surcharge = transportation.Surcharge ?? 0,
                                          Status = transportation.Status,
                                          AccountId = account.Id,
                                          AccountName = account.Username,
                                          Type = transportation.Type,
                                          Image = transportation.Image,
                                      }
                                      ).FirstOrDefaultAsync();
            if (scanResponse?.Status > (int)ETransportationStatus.ArrivedAtTQWarehouse)
            {
                throw new AppException("Mã vận đơn đang vận chuyển về Kho VN");
            }
            if (scanResponse?.Type != null && scanResponse?.Type != type)
            {
                throw new AppException($"Mã vận đơn thuộc loại {ETransportationTypeName.GetTypeName(scanResponse.Type ?? 0)}");
            }
            if (scanResponse != null)
                await _signalRService.SendScanWebMessage(JsonConvert.SerializeObject(scanResponse).ToString());
            return scanResponse;
        }

        public async Task<List<ScanResponse>> GetBarcodeInTQ(int type)
        {
            var scanResponse = await (from transportation in _unitOfWork.Repository<Transportation>().GetQueryable()
                                      join account in _unitOfWork.Repository<Account>().GetQueryable() on transportation.AccountId equals account.Id into accountJoin
                                      from account in accountJoin.DefaultIfEmpty()
                                      where transportation.Type == type && transportation.Status == (int)ETransportationStatus.ArrivedAtTQWarehouse
                                      select new ScanResponse
                                      {
                                          Id = transportation.Id,
                                          Barcode = transportation.Barcode,
                                          Quantity = transportation.Quantity ?? 0,
                                          Weight = transportation.Weight ?? 0,
                                          Volume = transportation.Volume ?? 0,
                                          Surcharge = transportation.Surcharge ?? 0,
                                          Status = transportation.Status,
                                          AccountId = account.Id,
                                          AccountName = account.Username,
                                          Type = transportation.Type,
                                          Image = transportation.Image,
                                      }
                                      ).ToListAsync();
            return scanResponse;
        }

        public async Task<ScanResponse> ScanByBarcodeAtDestinationWarehouse(string barcode, int? bigPackageId)
        {
            var scanResponse = await (from transportation in _unitOfWork.Repository<Transportation>().GetQueryable()
                                      join account in _unitOfWork.Repository<Account>().GetQueryable() on transportation.AccountId equals account.Id into accountJoin
                                      from account in accountJoin.DefaultIfEmpty()
                                      join bigPackage in _unitOfWork.Repository<BigPackage>().GetQueryable() on transportation.BigPackageId equals bigPackage.Id into bigPackageJoin
                                      from bigPackage in bigPackageJoin.DefaultIfEmpty()
                                      where transportation.Barcode == barcode.Trim()
                                      //&& (bigPackageId == null || transportation.BigPackageId == bigPackageId )
                                      select new ScanResponse
                                      {
                                          Id = transportation.Id,
                                          Barcode = transportation.Barcode,
                                          Quantity = transportation.Quantity ?? 0,
                                          Weight = transportation.Weight ?? 0,
                                          Volume = transportation.Volume ?? 0,
                                          Surcharge = transportation.Surcharge ?? 0,
                                          Status = transportation.Status,
                                          AccountId = account.Id,
                                          AccountName = account.Username,
                                          StaffNote = transportation.StaffNote,
                                          BigPackageId = bigPackage.Id,
                                          BigPackageName = bigPackage.Name,
                                      }
                                      ).FirstOrDefaultAsync();
            if (scanResponse?.Status > (int)ETransportationStatus.ArrivedAtVNWarehouse)
            {
                throw new AppException("Mã vận đơn đã xuất kho");
            }
            return scanResponse;
        }

        public async Task<List<TransportationResponse>> GetAtOutOfStock(int accountId)
        {
            var loggedModel = _httpContextService.GetLoggedModel();

            var query = from transportation in _unitOfWork.Repository<Transportation>().GetQueryable()
                        join account in _unitOfWork.Repository<Account>().GetQueryable() on transportation.AccountId equals account.Id into accountJoin
                        from account in accountJoin.DefaultIfEmpty()
                        where transportation.Status == (int)ETransportationStatus.ArrivedAtVNWarehouse
                            && transportation.AccountId == accountId
                            && (loggedModel.PostOffice == "" || transportation.PostOffice == loggedModel.PostOffice)
                        where !_unitOfWork.Repository<TransportationOutOfStock>().GetQueryable()
                                .Join(_unitOfWork.Repository<OutOfStock>().GetQueryable(), tos => tos.OutOfStockId, os => os.Id,
                                    (tos, os) => new { tos.TransportationId, os.Status })
                                .Any(x => x.TransportationId == transportation.Id && x.Status != (int)EOutOfStockStatus.Cancel)
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
                            Created = transportation.Created,
                            AccountId = transportation.AccountId,
                            AccountName = account != null ? account.Username : string.Empty,
                        };

            return await query.OrderByDescending(x => x.Id).ToListAsync();
        }

        public async Task<TransportationResponse> GetById(int id)
        {

            var transportationResponse = await (from transportation in _unitOfWork.Repository<Transportation>().GetQueryable()
                                                join account in _unitOfWork.Repository<Account>().GetQueryable() on transportation.AccountId equals account.Id into accountJoin
                                                from account in accountJoin.DefaultIfEmpty()
                                                where transportation.Id == id
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
                                                    AccountId = transportation.AccountId,
                                                    AccountName = account != null ? account.Username : string.Empty,
                                                    FromId = transportation.FromId,
                                                    ToId = transportation.ToId,
                                                    ShipId = transportation.ShipId,
                                                    StaffId = transportation.StaffId,

                                                    UserUploadWeight = transportation.UserUploadWeight,
                                                    UserUploadVolume = transportation.UserUploadVolume,
                                                    UserUploadQuantity = transportation.UserUploadQuantity,
                                                    UserUploadImage = transportation.UserUploadImage,
                                                }
                                 ).SingleOrDefaultAsync();
            var currentAccount = _httpContextService.GetLoggedModel();
            if (currentAccount.RoleId == (int)ERoleId.Sale && (transportationResponse.StaffId ?? currentAccount.Id) != currentAccount.Id)
            {
                throw new AppException("Đơn không thuộc tài khoản");
            }
            transportationResponse.TransportationHistories = await _unitOfWork.Repository<TransportationHistory>().GetQueryable().Where(x => x.TransportationId == id).OrderByDescending(x => x.Id).ToListAsync();
            transportationResponse.Products = await _unitOfWork.Repository<TransportationProduct>().GetQueryable().Where(x => x.TransportationId == id).OrderByDescending(x => x.Id).ToListAsync();
            return transportationResponse;
        }

        public async Task<bool> Update(int id, UpdateTransportationRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var currentAccount = await _httpContextService.GetCurrentAccount();
                var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
                var histories = new List<TransportationHistory>();

                var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.AccountId);
                var requestAccountQuery = _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.Username == (request.Username ?? "").Trim());

                if (currentAccount.RoleId == (int)ERoleId.Sale)
                {
                    requestAccountQuery = requestAccountQuery.Where(x => x.SaleId == currentAccount.Id);
                }
                var requestAccount = await requestAccountQuery.SingleOrDefaultAsync();
                if (currentAccount.RoleId == (int)ERoleId.Sale && !string.IsNullOrEmpty(request.Username) && requestAccount == null)
                {
                    throw new AppException("Không tìm thấy tài khoản");
                }
                var acceptRoles = new List<int>() { (int)ERoleId.Admin, (int)ERoleId.Manager, (int)ERoleId.Sale };
                if (transportation.AccountId != requestAccount?.Id && acceptRoles.Contains(currentAccount.RoleId))
                {
                    if (transportation.Status == (int)ETransportationStatus.Completed)
                    {
                        request.Status = (int)ETransportationStatus.ArrivedAtVNWarehouse;
                    }
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_ACCOUNT, currentAccount.Username, transportation.Barcode, customerAccount?.Username, request.Username)
                    });
                    transportation.AccountId = requestAccount?.Id;
                    customerAccount = requestAccount;
                }

                if (transportation.Status != request.Status)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_TRANG_THAI_DON, currentAccount.Username, transportation.Barcode, ETransportationStatusName.GetStatusName(transportation.Status), ETransportationStatusName.GetStatusName(request.Status))
                    });
                }

                if (customerAccount?.MinWeight != null)
                {
                    if (request.Weight < customerAccount.MinWeight)
                    {
                        request.Weight = customerAccount.MinWeight ?? request.Weight;
                    }
                }
                else
                {
                    if (request.Weight < 1)
                    {
                        request.Weight = 1;
                    }
                }

                if ((transportation.Weight ?? 0) != request.Weight)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_CAN_NANG_DON, currentAccount.Username, transportation.Barcode, transportation.Weight ?? 0, request.Weight)
                    });
                }
                if ((transportation.Volume ?? 0) != request.Volume)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_SO_KHOI_DON, currentAccount.Username, transportation.Barcode, transportation.Volume ?? 0, request.Volume)
                    });
                }
                if ((transportation.Quantity ?? 1) != request.Quantity)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_SO_KIEN_DON, currentAccount.Username, transportation.Barcode, transportation.Quantity ?? 0, request.Quantity)
                    });
                }
                if (transportation.Currency != request.Currency)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_TY_GIA_DON, currentAccount.Username, transportation.Barcode, string.Format("{0:N0}", transportation.Currency), string.Format("{0:N0}", request.Currency))
                    });
                }
                if ((transportation.Surcharge ?? 0) != request.Surcharge)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_PHU_PHI_DON, currentAccount.Username, transportation.Barcode, string.Format("{0:N2}", transportation.Surcharge), string.Format("{0:N2}", request.Surcharge))
                    });
                }
                if (transportation.FromId != request.FromId)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_KHO_NHAN_DON, currentAccount.Username, transportation.Barcode, transportation.FromId ?? 0, request.FromId)
                    });
                }
                if (transportation.ToId != request.ToId)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_KHO_DICH_DON, currentAccount.Username, transportation.Barcode, transportation.ToId ?? 0, request.ToId)
                    });
                }
                if (transportation.ShipId != request.ShipId)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_PTVC_DON, currentAccount.Username, transportation.Barcode, transportation.ShipId ?? 0, request.ShipId)
                    });
                }
                if (transportation.UnitWeight != request.UnitWeight)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_DON_GIA_CAN, currentAccount.Username, transportation.Barcode, string.Format("{0:N0}", transportation.UnitWeight), string.Format("{0:N0}", request.UnitWeight))
                    });
                }
                if (transportation.UnitVolume != request.UnitVolume)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = string.Format(HistoryContent.DOI_DON_GIA_KHOI, currentAccount.Username, transportation.Barcode, string.Format("{0:N0}", transportation.UnitVolume), string.Format("{0:N0}", request.UnitVolume))
                    });
                }

                var image = transportation.UserUploadImage;
                if (request.Images != null)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = id,
                        Content = $"{currentAccount.Username} đã đổi hình ảnh của đơn hàng"
                    });
                    var listImageUrl = new List<string>();
                    foreach (var itemImage in request.Images)
                    {
                        listImageUrl.Add(await _uploadFileService.UploadFile(itemImage));
                    }
                    image = string.Join('|', listImageUrl);
                    transportation.UserUploadImage = image;
                }
                _mapper.Map(request, transportation);
                decimal? priceWeight = transportation.UnitWeight * (decimal)transportation.Weight;
                decimal? priceVolume = transportation.UnitVolume * (decimal)transportation.Volume;
                transportation.PriceShipping = priceWeight > priceVolume ? priceWeight : priceVolume;
                decimal surchargeVND = Math.Round((transportation.Surcharge * transportation.Currency) ?? 0, 0);
                transportation.TotalPriceVND = (transportation.PriceShipping ?? 0) + surchargeVND;
                var voucherAccount = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.VoucherId && x.Status == (int)EVoucherAccountStatus.Used);
                transportation.Discount = voucherAccount?.Amount ?? 0;
                if (transportation.Discount > transportation.TotalPriceVND)
                {
                    transportation.Discount = transportation.TotalPriceVND;
                }
                transportation.TotalPriceVND -= transportation.Discount;
                transportation = await ChangeStatusDateAsync(transportation, currentAccount.Username, currentDate, currentAccount.Id);

                _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
                var bigPackage = await CalculateBigPackageInfor(transportation.BigPackageId ?? 0);
                if (bigPackage != null)
                {
                    _unitOfWork.Repository<BigPackage>().Update(bigPackage, currentDate, currentAccount.Id);
                }
                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitAsync();

                await _signalRService.SendConfirmNotification($"UpdatedBarcode-${transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<bool> UpdateUserUploadImage(UpdateTransportationUserUploadImageRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode);
            if (transportation == null)
                throw new AppException("Không tìm thấy đơn hàng");
            var histories = new List<TransportationHistory>();
            var image = transportation.UserUploadImage;
            if (request.Images != null)
            {
                histories.Add(new TransportationHistory
                {
                    TransportationId = transportation.Id,
                    Content = $"{currentAccount.Username} đã đổi hình ảnh của đơn hàng"
                });
                var listImageUrl = new List<string>();
                foreach (var itemImage in request.Images)
                {
                    listImageUrl.Add(await _uploadFileService.UploadFile(itemImage));
                }
                image = string.Join('|', listImageUrl);
                transportation.UserUploadImage = image;
            }
            _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
            await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            await _signalRService.SendConfirmNotification($"UpdatedBarcode-${transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");

            return true;
        }
        public async Task<bool> UpdateAtOutOfStockManage(int outOfStockId, UpdateTransportationAtOutOfStockManageRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                var currentAccount = await _httpContextService.GetCurrentAccount();
                var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode);
                var histories = new List<TransportationHistory>();

                //var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.AccountId);
                //if (customerAccount?.MinWeight != null)
                //{
                //    if (request.Weight < customerAccount.MinWeight)
                //    {
                //        request.Weight = customerAccount.MinWeight ?? request.Weight;
                //    }
                //}
                //else
                //{
                //    if (request.Weight < 1)
                //    {
                //        request.Weight = 1;
                //    }
                //}
                if (transportation.Weight != request.Weight)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_CAN_NANG_DON, currentAccount.Username, transportation.Barcode, transportation.Weight ?? 0, request.Weight)
                    });
                }
                if (transportation.Volume != request.Volume)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_SO_KHOI_DON, currentAccount.Username, transportation.Barcode, transportation.Volume ?? 0, request.Volume)
                    });
                }
                if (transportation.UnitWeight != request.UnitWeight)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_DON_GIA_CAN, currentAccount.Username, transportation.Barcode, string.Format("{0:N0}", transportation.UnitWeight), string.Format("{0:N0}", request.UnitWeight))
                    });
                }
                if (transportation.UnitVolume != request.UnitVolume)
                {
                    histories.Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_DON_GIA_KHOI, currentAccount.Username, transportation.Barcode, string.Format("{0:N0}", transportation.UnitVolume), string.Format("{0:N0}", request.UnitVolume))
                    });
                }
                _mapper.Map(request, transportation);
                decimal? priceWeight = transportation.UnitWeight * (decimal)transportation.Weight;
                decimal? priceVolume = transportation.UnitVolume * (decimal)transportation.Volume;
                transportation.PriceShipping = priceWeight > priceVolume ? priceWeight : priceVolume;
                decimal surchargeVND = Math.Round((transportation.Surcharge * transportation.Currency) ?? 0, 0);
                transportation.TotalPriceVND = (transportation.PriceShipping ?? 0) + surchargeVND;
                var voucherAccount = await _unitOfWork.Repository<VoucherAccount>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.VoucherId && x.Status == (int)EVoucherAccountStatus.Used);
                transportation.Discount = voucherAccount?.Amount ?? 0;
                if (transportation.Discount > transportation.TotalPriceVND)
                {
                    transportation.Discount = transportation.TotalPriceVND;
                }
                transportation.TotalPriceVND -= transportation.Discount;
                transportation = await ChangeStatusDateAsync(transportation, currentAccount.Username, currentDate, currentAccount.Id);

                _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();

                var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == outOfStockId);
                var tranportationOutOfStocks = await _unitOfWork.Repository<TransportationOutOfStock>().GetQueryable().Where(x => x.OutOfStockId == outOfStockId).ToListAsync();
                var transporations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => tranportationOutOfStocks.Select(x => x.TransportationId).ToList().Contains(x.Id)).ToListAsync();
                outOfStock.TotalPriceVND = transporations.Sum(x => x.TotalPriceVND);
                _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);

                var bigPackage = await CalculateBigPackageInfor(transportation.BigPackageId ?? 0);
                if (bigPackage != null)
                {
                    _unitOfWork.Repository<BigPackage>().Update(bigPackage, currentDate, currentAccount.Id);
                }
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();

                await _signalRService.SendConfirmNotification($"UpdatedBarcode-${transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException(ex.Message);
            }
        }

        public async Task<Transportation> ChangeStatusDateAsync(Transportation transportation, string currentAccountName, DateTime currentDate, int currentAccountId)
        {
            bool isSendNotification = false;
            switch (transportation.Status)
            {
                case (int)ETransportationStatus.ArrivedAtTQWarehouse:
                    if (transportation.DateArrivedAtTQWarehouse == null)
                    {
                        transportation.DateArrivedAtTQWarehouse = currentDate;
                        transportation.AccountArrivedAtTQWarehouse = currentAccountName;
                        var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Id == transportation.AccountId);
                        if (customerAccount != null)
                        {
                            string phone = customerAccount.Phone;
                            string customername = customerAccount.Username;
                            string mavandon = transportation.Barcode;
                            string thoigian = $"ngày {currentDate.Day} tháng {currentDate.Month} năm {currentDate.Year}";
                            var template = new TemplateData()
                            {
                                maVanDon = mavandon,
                                soKien = transportation.Quantity.ToString(),
                                username = customername,
                                thoiGian = thoigian,
                                ghiChu = transportation.UserNote
                            };
                            await _zaloAPIService.SendMessage(phone, "281630", template);
                        }
                        isSendNotification = true;
                    }
                    break;
                case (int)ETransportationStatus.ExitedFromTQWarehouse:
                    if (transportation.DateExitedFromTQWarehouse == null)
                    {
                        transportation.DateExitedFromTQWarehouse = currentDate;
                        transportation.AccountExitedFromTQWarehouse = currentAccountName;
                        isSendNotification = true;
                    }
                    break;
                case (int)ETransportationStatus.CustomsInspectedGoods:
                    if (transportation.DateCustomsInspectedGoods == null)
                    {
                        transportation.DateCustomsInspectedGoods = currentDate;
                        transportation.AccountCustomsInspectedGoods = currentAccountName;
                        isSendNotification = true;
                    }
                    break;
                case (int)ETransportationStatus.ReturningToVNWarehouse:
                    if (transportation.DateReturningToVNWarehouse == null)
                    {
                        transportation.DateReturningToVNWarehouse = currentDate;
                        transportation.AccountReturningToVNWarehouse = currentAccountName;
                        isSendNotification = true;
                    }
                    break;
                case (int)ETransportationStatus.ArrivedAtVNWarehouse:
                    if (transportation.DateArrivedAtVNWarehouse == null)
                    {
                        transportation.DateArrivedAtVNWarehouse = currentDate;
                        transportation.AccountArrivedAtVNWarehouse = currentAccountName;
                        isSendNotification = true;
                    }
                    break;
                case (int)ETransportationStatus.Completed:
                    if (transportation.DateCompleted == null)
                    {
                        transportation.DateCompleted = currentDate;
                        transportation.AccountCompleted = currentAccountName;
                        isSendNotification = true;
                    }
                    break;
                case (int)ETransportationStatus.Cancel:
                    if (transportation.DateCancel == null)
                    {
                        transportation.DateCancel = currentDate;
                        isSendNotification = true;
                    }
                    break;
                default:
                    break;
            }
            if (isSendNotification)
            {
                var notification = new Notification
                {
                    Title = "Trạng thái đơn hàng",
                    Content = $"Đơn hàng #{transportation.Id} đã đổi trạng thái sang {ETransportationStatusName.GetStatusName(transportation.Status)}",
                    WebUrl = $"/transportation-detail/{transportation.Id}",
                    Type = (int)ENotificationType.Order,
                    IsStaff = true,
                };
                await _notificationService.SendNotification(notification, currentDate, currentAccountId,
                    staffId: transportation.StaffId ?? 0, staffRoleIds: new List<int> { (int)ERoleId.Admin, (int)ERoleId.Manager, (int)ERoleId.TQWarehouseStaff, (int)ERoleId.VNWarehouseStaff, });
                var customerNotification = new Notification
                {
                    Title = "Trạng thái đơn hàng",
                    Content = $"Đơn hàng #{transportation.Id} đã đổi trạng thái sang {ETransportationStatusName.GetStatusName(transportation.Status)}",
                    WebUrl = $"/tracking?barcode={transportation.Barcode}",
                    Type = (int)ENotificationType.Order,
                    IsStaff = false
                };
                await _notificationService.SendNotification(customerNotification, currentDate, currentAccountId,
                    customerId: transportation.AccountId ?? 0);
            }
            return transportation;
        }

        public async Task<Transportation> GetByBarcode(string barcode)
        {
            return await _unitOfWork.Repository<Transportation>().GetQueryable().FirstOrDefaultAsync(t => t.Barcode == barcode);
        }
        private static string GetFinalSqlQuery(string sql, List<SqlParameter> parameters)
        {
            foreach (var param in parameters)
            {
                string valueStr;
                if (param.Value == DBNull.Value)
                {
                    valueStr = "NULL";
                }
                else if (param.Value is string || param.Value is DateTime)
                {
                    valueStr = $"'{param.Value}'"; // Thêm dấu nháy đơn nếu là string hoặc datetime
                }
                else
                {
                    valueStr = param.Value.ToString(); // Nếu là số thì giữ nguyên
                }

                sql = sql.Replace(param.ParameterName, valueStr);
            }
            return sql;
        }

        public async Task<bool> DeleteSelected(List<int> ids)
        {
            var transportations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => ids.Contains(x.Id)).ToListAsync();
            return await DeleteTransportation(transportations);
        }

        public async Task<bool> DeleteFilterd(TransportationSearch search)
        {
            bool hasFilter =
                search?.Status != null ||
                search?.IsNotAccount != null ||
                !string.IsNullOrWhiteSpace(search?.PostOffice) ||
                !string.IsNullOrWhiteSpace(search?.Barcode) ||
                search?.FromDate != null ||
                search?.ToDate != null;

            if (!hasFilter)
            {
                throw new AppException("Bạn phải chọn ít nhất một tiêu chí lọc trước khi xoá.");
            }
            var transportations = await _unitOfWork.Repository<Transportation>().GetQueryable()
                .Where(x => (search.Status == null || x.Status == search.Status)
                    && (string.IsNullOrEmpty(search.Barcode) || x.Barcode.Contains(search.Barcode))
                    && (string.IsNullOrEmpty(search.PostOffice) || x.PostOffice == search.PostOffice)
                    && (search.FromDate == null || x.Created >= search.FromDate)
                    && (search.ToDate == null || x.Created <= search.ToDate)
                ).ToListAsync();
            return await DeleteTransportation(transportations);
        }

        public async Task<BigPackage> CalculateBigPackageInfor(int id)
        {
            var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            if (bigPackage != null)
            {
                var transportationQuery = _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => x.BigPackageId == bigPackage.Id && x.Status != (int)ETransportationStatus.Cancel);
                bigPackage.Quantity = await transportationQuery.SumAsync(x => x.Quantity ?? 0);
                bigPackage.Weight = await transportationQuery.SumAsync(x => x.Weight ?? 0);
                bigPackage.Volume = await transportationQuery.SumAsync(x => x.Volume ?? 0);
            }
            return bigPackage;
        }

        private async Task<bool> DeleteTransportation(List<Transportation> transportations)
        {
            var bigPackageIds = transportations.Where(x => x.BigPackageId > 0).Select(x => x.BigPackageId ?? 0).Distinct().ToList();
            var bigPackageMustDelete = new List<BigPackage>();
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Repository<Transportation>().DeleteRange(transportations);
                await _unitOfWork.SaveAsync();
                foreach (var bigPackageId in bigPackageIds)
                {
                    var bigPackage = await CalculateBigPackageInfor(bigPackageId);
                    if (bigPackage != null && !(bigPackage?.Quantity > 0))
                    {
                        bigPackageMustDelete.Add(bigPackage);
                    }
                }
                _unitOfWork.Repository<BigPackage>().DeleteRange(bigPackageMustDelete);
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

        public async Task<bool> UpdateUserNote(string barcode, string note)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            return await UpdateUserNoteForApi(barcode, note, currentAccount);
        }
        public async Task<bool> UpdateShipId(string barcode, int shipId)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            return await UpdateShipIdForApi(barcode, shipId, currentAccount);
        }
        public async Task<bool> UpdateUserNoteForApi(string barcode, string note, Account currentAccount)
        {
            var currentDate = DateTime.Now;
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == barcode && x.Status <= (int)ETransportationStatus.ArrivedAtTQWarehouse);
            if (transportation == null)
                throw new AppException("Không tìm thấy đơn hàng");
            var histories = new List<TransportationHistory>();
            if (!string.IsNullOrEmpty(note))
            {
                histories.Add(new TransportationHistory
                {
                    TransportationId = transportation.Id,
                    Content = $"{currentAccount.Username} đã đổi ghi chú của đơn hàng từ {transportation.UserNote} sang {note}"
                });
                transportation.UserNote = note;
            }
            _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
            await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            await _signalRService.SendConfirmNotification($"UpdatedBarcode-${transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");

            return true;
        }
        public async Task<bool> UpdateShipIdForApi(string barcode, int shipId, Account currentAccount)
        {
            var currentDate = DateTime.Now;
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == barcode && x.AccountId == currentAccount.Id && x.Status <= (int)ETransportationStatus.ArrivedAtTQWarehouse);
            if (transportation == null)
                throw new AppException("Không tìm thấy đơn hàng");
            var ship = await _unitOfWork.Repository<Warehouse>().GetQueryable().SingleOrDefaultAsync(x => x.Id == shipId && x.Type == (int)EWarehouseType.Shipping);
            if (ship == null)
                throw new AppException("Không tìm thấy phương thức");
            var histories = new List<TransportationHistory>();
            if (shipId != transportation.ShipId)
            {
                histories.Add(new TransportationHistory
                {
                    TransportationId = transportation.Id,
                    Content = $"{currentAccount.Username} đã đổi PTVC của đơn hàng từ {transportation.ShipId} sang {shipId}"
                });
                transportation.ShipId = shipId;
            }
            _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
            await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            await _signalRService.SendConfirmNotification($"UpdatedBarcode-${transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");

            return true;
        }
        // public async Task<object> SearchCode(string? name, string? hscode)
        // {
        //     var query = _unitOfWork.Repository<Transportation>().GetQueryable();

        //     if (!string.IsNullOrWhiteSpace(name))
        //     {
        //         query = query.Where(x => x.UserNote.Contains(name));
        //     }

        //     if (!string.IsNullOrWhiteSpace(hscode))
        //     {
        //         query = query.Where(x => x.hscode == hscode);
        //     }

        //     var data = await query.Take(20)
        //         .Select(x => new
        //         {
        //             x.Id,
        //             x.UserNote,
        //             x.TotalPriceVND,
        //             x.PostOffice,
        //             x.Barcode
        //         })
        //         .ToListAsync();

        //     return new
        //     {
        //         success = true,
        //         data
        //     };
        // }
    }
}
