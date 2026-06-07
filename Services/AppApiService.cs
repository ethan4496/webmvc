using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Services
{
    public class AppApiService : IAppApiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ISendEmailService _sendEmailService;
        private readonly ITransportationService _transportationService;
        private readonly IWarehouseService _warehouseService;
        private readonly IUploadFileService _uploadFileService;
        private readonly ITrackingService _trackingService;
        private readonly IOutOfStockService _outOfStockService;
        private readonly IBigPackageService _bigPackageService;
        private readonly ISignalRService _signalRService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AppApiService(IUnitOfWork unitOfWork, IConfiguration configuration, IMapper mapper,
            INotificationService notificationService, ISendEmailService sendEmailService,
            ITransportationService transportationService, IWarehouseService warehouseService,
            IUploadFileService uploadFileService, ITrackingService trackingService, IOutOfStockService outOfStockService,
            IBigPackageService bigPackageService, ISignalRService signalRService,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mapper = mapper;
            _notificationService = notificationService;
            _sendEmailService = sendEmailService;
            _transportationService = transportationService;
            _warehouseService = warehouseService;
            _uploadFileService = uploadFileService;
            _trackingService = trackingService;
            _outOfStockService = outOfStockService;
            _bigPackageService = bigPackageService;
            _signalRService = signalRService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseClass> UpdateUserUploadImage(UpdateUserNoteRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }

            var currentDate = DateTime.Now;
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode && x.Status <= (int)ETransportationStatus.ArrivedAtTQWarehouse);
            if (transportation == null)
                throw new AppException("Không tìm thấy đơn hàng");
            var histories = new List<TransportationHistory>();
            var image = transportation.UserUploadImage;
            if (request.Note != null)
            {
                histories.Add(new TransportationHistory
                {
                    TransportationId = transportation.Id,
                    Content = $"{currentAccount.Username} đã đổi file SP của đơn hàng"
                });
                transportation.UserUploadImage = request.Note;
            }
            _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
            await _unitOfWork.Repository<TransportationHistory>().AddRange(histories, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            await _signalRService.SendConfirmNotification($"UpdatedBarcode-${transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");


            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.Message = "Cập nhật thành công";
            return rs;
        }

        public async Task<ResponseClass> UpdateShipId(UpdateShipIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            await _transportationService.UpdateShipIdForApi(request.Barcode, request.ShipId, currentAccount);
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.Message = "Cập nhật thành công";
            return rs;
        }
        public async Task<ResponseClass> UpdateUserNote(UpdateUserNoteRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            await _transportationService.UpdateUserNoteForApi(request.Barcode, request.Note, currentAccount);
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.Message = "Cập nhật thành công";
            return rs;
        }


        public async Task<ResponseClass> GetSmallPackage(TrackingRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode.Trim());
            if (transportation == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
                rs.data = null;
                return rs;
            }
            var ship = await _unitOfWork.Repository<Warehouse>().GetQueryable().FirstOrDefaultAsync(x => x.Id == transportation.ShipId);
            var smallPackage = new SmallPackage()
            {
                ID = transportation.Id,
                Quantity = transportation.Quantity ?? 0,
                ProductQuantity = (transportation.Quantity ?? 1).ToString(),
                Weight = transportation.Weight ?? 0,
                Volume = transportation.Volume ?? 0,
                AdditionFeeCYN = (double)transportation.Surcharge,
                IMGPackage = transportation.Image,
                ShippingType = transportation.ShipId,
                ShippingTypeName = ship?.Name,
                Status = transportation.Status,
                BigPackageId = transportation.BigPackageId,
                OrderTransactionCode = transportation.Barcode
            };
            var customer = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.AccountId);
            if (customer != null)
            {
                smallPackage.Username = customer.Username;
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = smallPackage;
            return rs;
        }

        public async Task<ResponseClass> UpdateSmallPackagesInVN(UpdateSmallPackageAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            if (currentAccount.RoleId == (int)ERoleId.Sale)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Không có quyền";
                return rs;
            }
            var listUpdateBarcodeRequest = new List<UpdateBarcodeRequest>();
            foreach (var smallPackage in request.SmallPackages)
            {
                listUpdateBarcodeRequest.Add(new UpdateBarcodeRequest
                {
                    Id = smallPackage.ID,
                    ShipId = smallPackage.ShippingType,
                    Quantity = smallPackage.Quantity,
                    Weight = smallPackage.Weight,
                    Volume = smallPackage.Volume,
                    Surcharge = (decimal)(smallPackage.AdditionFeeCYN ?? 0),
                    Image = smallPackage.IMGPackage,
                });
            }
            if (await _transportationService.UpdateBarcodeMultipleForApi(listUpdateBarcodeRequest, (int)ETransportationStatus.ArrivedAtVNWarehouse, request.Type, DateTime.Now, currentAccount))
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
            }
            else
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Tạo thất bại";
            }
            return rs;
        }

        public async Task<ResponseClass> UpdateSmallPackages(UpdateSmallPackageAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            if (currentAccount.RoleId == (int)ERoleId.Sale)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Không có quyền";
                return rs;
            }
            var listUpdateBarcodeRequest = new List<UpdateBarcodeRequest>();
            foreach (var smallPackage in request.SmallPackages)
            {
                listUpdateBarcodeRequest.Add(new UpdateBarcodeRequest
                {
                    Id = smallPackage.ID,
                    ShipId = smallPackage.ShippingType,

                    Quantity = smallPackage.Quantity,
                    Weight = smallPackage.Weight,
                    Volume = smallPackage.Volume,
                    Surcharge = (decimal)(smallPackage.AdditionFeeCYN ?? 0),
                    Image = smallPackage.IMGPackage,
                });
            }
            if (await _transportationService.UpdateBarcodeMultipleForApi(listUpdateBarcodeRequest, (int)ETransportationStatus.ArrivedAtTQWarehouse, request.Type, DateTime.Now, currentAccount))
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
            }
            else
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Tạo thất bại";
            }
            return rs;
        }

        public async Task<ResponseClass> CreateSmallPackage(CreateSmallPackageAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            if (currentAccount.RoleId == (int)ERoleId.Sale)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Không có quyền";
                return rs;
            }
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode.Trim());
            if (transportation != null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Mã vận đơn đã tồn tại";
                return rs;
            }
            try
            {
                var currentDate = DateTime.Now;
                var customerAccount = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Username == request.Username.Trim());
                //if (customerAccount == null)
                //{
                //    rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                //    rs.Status = APIUtils.ResponseMessage.Error.ToString();
                //    rs.Message = "Khách hàng không tồn tại";
                //    return rs;
                //}
                var config = await _unitOfWork.Repository<WebConfiguration>().GetQueryable().SingleOrDefaultAsync();
                decimal currency = config.Currency;
                await _unitOfWork.BeginTransactionAsync();

                int shipId = 4;
                var listAccountCN = new List<int>() { 20398, 20400 };
                var listAccountHT = new List<int>() { 15780, 15338 };
                var listAccountDT = new List<int>() { 20401 };
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

                transportation = new Transportation
                {
                    Barcode = request.Barcode,
                    UserNote = "",
                    Status = (int)ETransportationStatus.New,
                    Type = request.Type,
                    Weight = 0,
                    Volume = 0,
                    Quantity = 1,
                    Currency = currency,
                    Surcharge = 0,
                    PriceShipping = 0,
                    TotalPriceVND = 0,
                    PostOffice = customerAccount?.PostOffice ?? PostOfficeName.GetPostOffice().FirstOrDefault(),
                    AccountId = customerAccount?.Id,

                    FromId = 1,
                    ToId = 2,
                    ShipId = shipId,
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
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
                rs.data = new SmallPackageOfListApp
                {
                    ID = transportation.Id,
                };
                return rs;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = ex.Message;
                return rs;
            }
        }

        public async Task<ResponseClass> GetListSmallPackage(ListSmallPackageRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            int PageSize = 20;
            var smallPackagePackageOfListApps = new List<SmallPackageOfListApp>();
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(request.FD))
            {
                fromDate = DateTime.ParseExact(request.FD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(request.TD))
            {
                toDate = DateTime.ParseExact(request.TD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            var searchRequest = new TransportationSearch
            {
                Barcode = string.IsNullOrEmpty(request.Code.Trim()) ? null : request.Code,
                Status = (int)ETransportationStatus.ArrivedAtTQWarehouse,
                FromDate = fromDate,
                ToDate = toDate,
                PageIndex = request.PageIndex + 1,
                PageSize = PageSize,
                Type = request.Type,
            };
            var loggedModel = new LoggedModel
            {
                Id = currentAccount.Id,
                IsStaff = 1,
                RoleId = currentAccount.RoleId,
            };

            var pagedData = await _transportationService.GetPagingForAPI(searchRequest, loggedModel);
            if (pagedData.TotalItem > 0)
            {
                foreach (var transportation in pagedData.Items)
                {
                    smallPackagePackageOfListApps.Add(new SmallPackageOfListApp
                    {
                        ID = transportation.Id,
                        OrderTransactionCode = transportation.Barcode,
                        ProductQuantity = transportation.Quantity.ToString(),
                        Volume = transportation.Volume ?? 0,
                        Weight = transportation.Weight ?? 0,
                        DateInTQWarehouse = transportation.DateArrivedAtTQWarehouse
                    });
                }
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = smallPackagePackageOfListApps;
            rs.TotalPage = pagedData.TotalPage;
            rs.TotalItem = pagedData.TotalItem;
            return rs;
        }

        public async Task<ResponseClass> CreateBigPackage(CreateBigPackageAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Name == request.Code.Trim());
            if (bigPackage != null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Bao hàng đã tồn tại";
                return rs;
            }
            var createBigPackageRequest = new CreateBigPackageRequest
            {
                Name = request.Code,
                Quantity = request.TotalPackage,
                Volume = request.TotalVolume,
                Weight = request.TotalWeight,
                Partner = request.PartnerInfo,
                TransporationIds = request.SmallPackageIDs
            };
            if (await _bigPackageService.CreateBigPackageForApi(createBigPackageRequest, DateTime.Now, currentAccount))
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
            }
            else
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Tạo bao thất bại";
            }
            return rs;
        }

        public async Task<ResponseClass> GetBigPackageByID(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID);
            if (bigPackage == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var bigPackageDetail = new BigPackageDetailApp
            {
                ID = bigPackage.Id,
                PackageCode = bigPackage.Name,
                Status = bigPackage.Status,
                TotalPackage = bigPackage.Quantity,
                Volume = bigPackage.Volume,
                Weight = bigPackage.Weight,
                PartnerInfor = bigPackage.Partner
            };
            var transportations = await _unitOfWork.Repository<Transportation>().GetQueryable().Where(x => x.BigPackageId == bigPackage.Id).ToListAsync();
            if (transportations.Any())
            {
                foreach (var transportation in transportations)
                {
                    bigPackageDetail.SmallPackages.Add(new SmallPachageBigPackageDetail
                    {
                        ID = transportation.Id,
                        OrderTransactionCode = transportation.Barcode,
                        ProductQuantity = transportation.Quantity.ToString(),
                        Volume = transportation.Volume ?? 0,
                        Weight = transportation.Weight ?? 0,
                        DateInTQWarehouse = transportation.DateArrivedAtTQWarehouse,
                    });
                }
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = bigPackageDetail;
            return rs;
        }

        public async Task<ResponseClass> AddSmallPackageToBigPackage(AddSmallPackageToBigPackageRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.BigPackageID);
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode.Trim());
            if (bigPackage == null || transportation == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                return rs;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                int? oldBigPackageId = transportation.BigPackageId;
                transportation.BigPackageId = bigPackage.Id;
                await _unitOfWork.Repository<TransportationHistory>().Add(new TransportationHistory
                {
                    TransportationId = transportation.Id,
                    Content = string.Format(HistoryContent.GAN_BAO, currentAccount.Username, transportation.Id, bigPackage.Name)
                }, currentDate, currentAccount.Id);
                if (transportation.Status != bigPackage.Status)
                {
                    await _unitOfWork.Repository<TransportationHistory>().Add(new TransportationHistory
                    {
                        TransportationId = transportation.Id,
                        Content = string.Format(HistoryContent.DOI_TRANG_THAI_DON, currentAccount.Username, transportation.Barcode, ETransportationStatusName.GetStatusName(transportation.Status), ETransportationStatusName.GetStatusName(bigPackage.Status))
                    }, currentDate, currentAccount.Id);
                    transportation.Status = bigPackage.Status;
                }
                _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
                var listbigPackageUpdate = new List<BigPackage>();
                var listbigPackageUpdateHistory = new List<BigPackageHistory>();
                bigPackage = await _transportationService.CalculateBigPackageInfor(bigPackage.Id);
                listbigPackageUpdateHistory.Add(new BigPackageHistory
                {
                    BigPackageId = bigPackage.Id,
                    Content = string.Format(HistoryContent.GAN_BAO, currentAccount.Username, transportation.Id, bigPackage.Name),
                });
                listbigPackageUpdateHistory.Add(new BigPackageHistory
                {
                    BigPackageId = bigPackage.Id,
                    Content = string.Format(HistoryContent.DOI_THONG_TIN_BAO, currentAccount.Username, bigPackage.Name, bigPackage.Partner, bigPackage.Quantity, bigPackage.Weight, bigPackage.Volume),
                });
                listbigPackageUpdate.Add(bigPackage);
                var bigPackageOld = await _transportationService.CalculateBigPackageInfor(oldBigPackageId ?? 0);
                if (bigPackageOld != null)
                {
                    listbigPackageUpdate.Add(bigPackageOld);
                    listbigPackageUpdateHistory.Add(new BigPackageHistory
                    {
                        BigPackageId = bigPackageOld.Id,
                        Content = string.Format(HistoryContent.BO_GAN_BAO, currentAccount.Username, transportation.Id, bigPackageOld.Name),
                    });
                    listbigPackageUpdateHistory.Add(new BigPackageHistory
                    {
                        BigPackageId = bigPackageOld.Id,
                        Content = string.Format(HistoryContent.DOI_THONG_TIN_BAO, currentAccount.Username, bigPackageOld.Name, bigPackageOld.Partner, bigPackageOld.Quantity, bigPackageOld.Weight, bigPackageOld.Volume),
                    });
                }
                _unitOfWork.Repository<BigPackage>().UpdateRange(listbigPackageUpdate, currentDate, currentAccount.Id);
                await _unitOfWork.Repository<BigPackageHistory>().AddRange(listbigPackageUpdateHistory, currentDate, currentAccount.Id);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();

                await _signalRService.SendConfirmNotification($"UpdatedBarcode-{transportation.Barcode}", $"Đơn hàng đã được {currentAccount.Username} cập nhật, vui lòng xác nhận để tài lại dữ liệu");

                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
            }
            catch
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
            }
            return rs;
        }

        public async Task<ResponseClass> UpdateBigPackage(UpdateBigPackageAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID);
            if (bigPackage == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var updateBigPackgeRequest = new UpdateBigPackageRequest
            {
                Name = bigPackage.Name,
                Partner = request.PartnerInfor,
                Quantity = bigPackage.Quantity,
                Status = request.Status,
                Volume = request.TotalVolume,
                Weight = request.TotalWeight,
            };
            if (await _bigPackageService.UpdateForApi(bigPackage.Id, updateBigPackgeRequest, DateTime.Now, currentAccount))
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
            }
            else
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Lỗi cập nhật bao hàng";
            }
            return rs;
        }

        public async Task<ResponseClass> GetListBigPackage(ListBigPackageRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            int PageSize = 20;
            var bigPackagePackageOfListApps = new List<BigPackagePackageOfListApp>();
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(request.FD))
            {
                fromDate = DateTime.ParseExact(request.FD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(request.TD))
            {
                toDate = DateTime.ParseExact(request.TD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            var searchRequest = new BigPackageSearch
            {
                Name = string.IsNullOrEmpty(request.Code.Trim()) ? null : request.Code,
                Status = request.Status > -1 ? request.Status : null,
                FromDate = fromDate,
                ToDate = toDate,
                PageIndex = request.PageIndex + 1,
                PageSize = PageSize,
            };

            var pagedData = await _bigPackageService.GetPaging(searchRequest);
            if (pagedData.TotalItem > 0)
            {
                foreach (var item in pagedData.Items)
                {
                    var bigPackage = new BigPackagePackageOfListApp();
                    bigPackage.ID = item.Id;
                    bigPackage.PackageCode = item.Name;
                    bigPackage.Weight = item.Weight.ToString();
                    bigPackage.Volume = item.Volume.ToString();
                    bigPackage.TotalPackage = item.Quantity.ToString();
                    bigPackage.Status = item.Status;
                    bigPackage.StatusString = item.StatusName;
                    bigPackage.CreatedDate = FormatDate.FormatNullDate(item.Created);
                    bigPackage.PartnerInfor = item.Partner;
                    bigPackage.TotalDay = (DateTime.Now - item.Created).Days;
                    bigPackagePackageOfListApps.Add(bigPackage);
                }
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = bigPackagePackageOfListApps;
            rs.TotalPage = pagedData.TotalPage;
            rs.TotalItem = pagedData.TotalItem;
            return rs;
        }

        public async Task<ResponseClass> GetDataToCreateBigPackage(GetDataToCreateBigPackageRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(request.FD))
            {
                fromDate = DateTime.ParseExact(request.FD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(request.TD))
            {
                toDate = DateTime.ParseExact(request.TD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }
            var dataToCreateBigPackage = new DataToCreateBigPackage();

            var query = _unitOfWork.Repository<Transportation>().GetQueryable().Where(x =>
                                x.Status == (int)ETransportationStatus.ArrivedAtTQWarehouse
                                && (request.Type == 0 || x.Type == request.Type)
                                && (string.IsNullOrEmpty(request.Code) || x.Barcode == request.Code.Trim())
                                && (fromDate == null || x.DateArrivedAtTQWarehouse >= fromDate)
                                && (toDate == null || x.DateArrivedAtTQWarehouse <= toDate)
                    );
            dataToCreateBigPackage.TotalPackage = await query.SumAsync(x => x.Quantity ?? 0);
            dataToCreateBigPackage.TotalWeight = await query.SumAsync(x => x.Weight ?? 0);
            dataToCreateBigPackage.TotalVolume = await query.SumAsync(x => x.Volume ?? 0);
            dataToCreateBigPackage.SmallPackageIds = await query.Select(x => x.Id).ToListAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = dataToCreateBigPackage;
            return rs;
        }

        public async Task<ResponseClass> DeleteSmallPackage(TrackingRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode);
            if (transportation == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                return rs;
            }
            await _transportationService.DeleteSelected(new List<int> { transportation.Id });
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = "OK";
            return rs;
        }

        public async Task<ResponseClass> RollBackSmallPackage(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID);
            if (transportation == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var currentDate = DateTime.Now;
            await _unitOfWork.Repository<TransportationHistory>().Add(new TransportationHistory
            {
                TransportationId = transportation.Id,
                Content = string.Format(HistoryContent.DOI_TRANG_THAI_DON, currentAccount.Username, transportation.Barcode, ETransportationStatusName.GetStatusName(transportation.Status), ETransportationStatusName.GetStatusName((int)ETransportationStatus.ArrivedAtTQWarehouse))
            }, currentDate, currentAccount.Id);
            int bigPackageId = transportation.BigPackageId ?? 0;
            transportation.BigPackageId = null;
            transportation.Status = (int)ETransportationStatus.ArrivedAtTQWarehouse;

            _unitOfWork.Repository<Transportation>().Update(transportation, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            var bigPackage = await _transportationService.CalculateBigPackageInfor(bigPackageId);
            if (bigPackage != null)
            {
                _unitOfWork.Repository<BigPackage>().Update(bigPackage, currentDate, currentAccount.Id);
            }
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = "OK";
            return rs;
        }

        public async Task<ResponseClass> UpdateBigPackageName(UpdateBigPackageNameRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var bigPackage = await _unitOfWork.Repository<BigPackage>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID);
            if (bigPackage == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            bigPackage.Name = request.Name;
            await _unitOfWork.Repository<BigPackageHistory>().Add(new BigPackageHistory
            {
                BigPackageId = bigPackage.Id,
                Content = string.Format(HistoryContent.DOI_THONG_TIN_BAO, currentAccount.Username, request.Name, bigPackage.Partner, bigPackage.Quantity, bigPackage.Weight, bigPackage.Volume),

            }, DateTime.Now, currentAccount.Id);
            _unitOfWork.Repository<BigPackage>().Update(bigPackage, DateTime.Now, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }

        public async Task<ResponseClass> GetListAllSmallPackage(ListTransportationAppAdminRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            int PageSize = 20;
            var smallPackages = new List<SmallPackageOfListApp>();
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(request.FD))
            {
                fromDate = DateTime.ParseExact(request.FD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(request.TD))
            {
                toDate = DateTime.ParseExact(request.TD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            var searchRequest = new TransportationSearch
            {
                Barcode = string.IsNullOrEmpty(request.Code.Trim()) ? null : request.Code,
                Status = request.Status > -1 ? request.Status : null,
                FromDate = fromDate,
                ToDate = toDate,
                PageIndex = request.PageIndex + 1,
                PageSize = PageSize,
            };
            var loggedModel = new LoggedModel
            {
                Id = currentAccount.Id,
                IsStaff = 1,
                RoleId = currentAccount.RoleId,
            };
            var pagedData = await _transportationService.GetPagingForAPI(searchRequest, loggedModel);
            if (pagedData.TotalItem > 0)
            {
                foreach (var item in pagedData.Items)
                {
                    var smallPackage = new SmallPackageOfListApp();
                    smallPackage.ID = item.Id;
                    smallPackage.OrderTransactionCode = item.Barcode;
                    smallPackage.BigPackageName = item.BigPackageName;
                    smallPackage.PartnerInfor = item.PartnerInfor;
                    smallPackage.ProductQuantity = item.Quantity.ToString();
                    smallPackage.Weight = item.Weight ?? 0;
                    smallPackage.Volume = item.Volume ?? 0;
                    smallPackage.Status = item.Status;
                    smallPackage.CreatedDate = item.Created;
                    smallPackage.DateInTQWarehouse = item.DateArrivedAtTQWarehouse;
                    smallPackage.AccountInTQWarehouse = item.AccountArrivedAtTQWarehouse;
                    smallPackage.DateComingVNWarehouse = item.DateExitedFromTQWarehouse;
                    smallPackage.DateCheck = item.DateCustomsInspectedGoods;
                    smallPackage.DateInLasteWareHouse = item.DateArrivedAtVNWarehouse;
                    smallPackage.DateExport = item.DateCompleted;
                    smallPackage.AdditionFeeCYN = (double)(item.Surcharge ?? 0);
                    smallPackage.AdditionFeeVND = (double)item.SurchargeVND;
                    smallPackage.IMGPackage = item.Image;
                    smallPackage.UserUploadImage = item.UserUploadImage;
                    smallPackage.UserNote = item.UserNote;
                    smallPackages.Add(smallPackage);
                }
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = smallPackages;
            rs.TotalPage = pagedData.TotalPage;
            rs.TotalItem = pagedData.TotalItem;
            return rs;
        }

        public async Task<ResponseClass> ListExportRequestAdmin(ExportRequestTurnAdminRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(request.FD))
            {
                fromDate = DateTime.ParseExact(request.FD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(request.TD))
            {
                toDate = DateTime.ParseExact(request.TD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }
            var customer = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Username == request.Username.Trim());

            List<ExportRequestTurnOfListResponse> exportRequestTurnOfListResponses = new List<ExportRequestTurnOfListResponse>();
            var outOfStocks = await _unitOfWork.Repository<OutOfStock>().GetQueryable()
                .Where(x => x.AccountId == currentAccount.Id
                    && (request.Status == 0
                        || (request.Status == 1 && x.StatusPayment == (int)EPaymentOutOfStockStatus.New)
                        || (request.Status == 2 && x.StatusPayment == (int)EPaymentOutOfStockStatus.Paied)
                        || (request.Status == 3 && x.Status == (int)EOutOfStockStatus.New)
                        || (request.Status == 4 && x.Status == (int)EOutOfStockStatus.Done)
                        || (request.Status == 5 && x.IsRequest == true)
                    )
                    && (customer == null || x.AccountId == customer.Id)
                    && (request.ID == 0 || x.Id == request.ID)
                    && (fromDate == null || x.Created >= fromDate)
                    && (toDate == null || x.Created <= toDate)
                    && (request.PostOffice == "" || x.PostOffice == request.PostOffice)
                )
                .OrderByDescending(x=>x.Id)
                .Skip(request.PageIndex * 40)
                .Take(40)
                .ToListAsync();
            foreach (var outOfStock in outOfStocks)
            {
                exportRequestTurnOfListResponses.Add(new ExportRequestTurnOfListResponse
                {
                    ID = outOfStock.Id,
                    CreatedDate = outOfStock.Created,
                    IsRequest = outOfStock.IsRequest,
                    Note = outOfStock.DeliveryMethod,
                    Status = outOfStock.StatusPayment,
                    StatusExport = outOfStock.Status,
                    TotalPriceVND = outOfStock.TotalPriceVND,
                    PostOffice = outOfStock.PostOffice,
                    StaffNote = outOfStock.DeliveryInfo,
                    DateCallPhone = outOfStock.DateCallPhone,
                });
            }
            rs.data = exportRequestTurnOfListResponses;
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }

        public async Task<ResponseClass> UpdatePostOffice(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID);
            if (outOfStock == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            outOfStock.PostOffice = request.Reason;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, DateTime.Now, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> SendNotiAdmin(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID);
            if (outOfStock == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var currentDate = DateTime.Now;
            var customer = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == outOfStock.AccountId);
            string customerUsername = customer.Username, customerEmail = customer.Email;
            var fileHtml = await _outOfStockService.RenderDeliveryNote(new List<int> { request.ID });
            outOfStock.IsSend = true;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, currentDate, currentAccount.Id);
            if (await _unitOfWork.SaveAsync() > 0)
            {
                var customerNotification = new Notification
                {
                    Title = "Phiếu xuất kho",
                    Content = $"PXK {request.ID} Thanh Toán Ngay , nhận hàng liền tay!!!",
                    WebUrl = $"/out-of-stock",
                    Type = (int)ENotificationType.Order,
                    IsStaff = false
                };
                await _notificationService.SendNotification(customerNotification, currentDate, currentAccount.Id,
                    customerId: customer.Id);
                _outOfStockService.SendEmailDeliveryNote(request.ID, customerUsername, customerEmail, fileHtml);
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> OutStockAdmin(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            await _outOfStockService.ExportForAPI(request.ID, currentAccount);
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> UpdateCallPhone(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID);
            if (outOfStock == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            outOfStock.DateCallPhone = DateTime.Now;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, DateTime.Now, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }

        //public async Task<ResponseClass> UpdateSmallPackagesInVN(UpdateSmallPackageRequest request)
        //{
        //    var rs = new ResponseClass();
        //    var currentAccount = await GetSessionAsync(request);
        //    if (currentAccount == null)
        //    {
        //        rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
        //        rs.Status = APIUtils.ResponseMessage.Error.ToString();
        //        rs.Logout = "1";
        //        return rs;
        //    }

        //    rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
        //    rs.Status = APIUtils.ResponseMessage.Success.ToString();
        //    return rs;
        //}

        public async Task<ResponseClass> GetAllPXK(BaseAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var ids = await _unitOfWork.Repository<OutOfStock>().GetQueryable().Where(x => x.AccountId == currentAccount.Id && x.StatusPayment != (int)EPaymentOutOfStockStatus.Paied && x.Status != (int)EOutOfStockStatus.Cancel).Select(x => x.Id).ToListAsync();
            var fileHtml = await _outOfStockService.RenderDeliveryNote(ids);
            rs.data = await _uploadFileService.SavePXKToPdf(fileHtml, string.Join(",", ids));
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }

        public async Task<ResponseClass> GetPXKByID(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var fileHtml = await _outOfStockService.RenderDeliveryNote(new List<int> { request.ID });
            rs.data = await _uploadFileService.SavePXKToPdf(fileHtml, request.ID.ToString());
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }

        public async Task<ResponseClass> SendRequestOutStock(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            await _outOfStockService.SendRequestForAPI(new List<int> { request.ID }, currentAccount, DateTime.Now);
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> UpdateTTNH(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID);
            if (outOfStock == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            outOfStock.DeliveryInfo = request.Reason;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, DateTime.Now, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> UpdateHTGH(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var outOfStock = await _unitOfWork.Repository<OutOfStock>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID);
            if (outOfStock == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            outOfStock.DeliveryMethod = request.Reason;
            _unitOfWork.Repository<OutOfStock>().Update(outOfStock, DateTime.Now, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> ListExportRequest(ExportRequestTurnRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            List<ExportRequestTurnOfListResponse> exportRequestTurnOfListResponses = new List<ExportRequestTurnOfListResponse>();
            var outOfStocks = await _unitOfWork.Repository<OutOfStock>().GetQueryable()
                .Where(x => x.Status != (int)EOutOfStockStatus.Cancel
                    && x.AccountId == currentAccount.Id
                    //&& (x.IsSend == true || (x.IsSend != true && x.Status == (int)EOutOfStockStatus.New))
                    && (x.IsSend == true || x.Status == (int)EOutOfStockStatus.Done)
                    && (request.ID == 0 || x.Id == request.ID)
                    )
                .OrderByDescending(x => x.Id)
                .Skip((request.PageIndex) * 40)
                .Take(40)
                .ToListAsync();
            foreach (var outOfStock in outOfStocks)
            {
                exportRequestTurnOfListResponses.Add(new ExportRequestTurnOfListResponse
                {
                    ID = outOfStock.Id,
                    CreatedDate = outOfStock.Created,
                    IsRequest = outOfStock.IsRequest,
                    Note = outOfStock.DeliveryMethod,
                    Status = outOfStock.StatusPayment,
                    StatusExport = outOfStock.Status,
                    TotalPriceVND = outOfStock.TotalPriceVND,
                    PostOffice = outOfStock.PostOffice,
                    StaffNote = outOfStock.DeliveryInfo
                });
            }
            rs.data = exportRequestTurnOfListResponses;
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> ListVoucher(BaseAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            List<VoucherAppResponse> vouchers = new List<VoucherAppResponse>();
            var voucherAccounts = await _unitOfWork.Repository<VoucherAccount>().GetQueryable()
                .Where(x => x.AccountId == currentAccount.Id && x.EndDate > DateTime.Now && x.Status == (int)EVoucherAccountStatus.New)
                .ToListAsync();
            foreach (var voucherAccount in voucherAccounts)
            {
                vouchers.Add(new VoucherAppResponse
                {
                    ID = voucherAccount.Id,
                    Image = voucherAccount.Image,
                    Code = "",
                    Name = voucherAccount.Name,
                    Description = voucherAccount.Description,
                    DecreaseAmount = string.Format("{0:N0}", voucherAccount.Amount),
                    EndDate = voucherAccount.EndDate.ToString("dd/MM/yyyy HH:mm"),
                    Expiration = (voucherAccount.EndDate.Subtract(DateTime.Now)).Days
                });
            }
            rs.data = vouchers;
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }

        public async Task<ResponseClass> UpdateNotification(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var currentDate = DateTime.Now;
            var notification = await _unitOfWork.Repository<Notification>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.ID && x.AccountId == currentAccount.Id);
            notification.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notification, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> UpdateAllNotification(BaseAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var currentDate = DateTime.Now;
            string sql = $"UPDATE Notifications SET IsRead = 1, Updated = '{currentDate}', UpdateBy = {currentAccount.Id} WHERE AccountId = {currentAccount.Id} AND IsRead = 0 ";
            await _unitOfWork.ExecuteSqlRawAsync(sql);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> TotalNotification(BaseAppRequest request, bool isStaff)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }

            var total = await _unitOfWork.Repository<Notification>().GetQueryable().Where(x =>
                x.AccountId == currentAccount.Id
                && x.IsStaff == isStaff
                && (isStaff == false || x.Title == "Đơn hàng mới")
                && x.IsRead == false
            ).CountAsync();
            rs.TotalPage = total;
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }

        public async Task<ResponseClass> TotalUnReadMessage(BaseAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://chat.tpkexpress.cn/get-unread-message?accountId={currentAccount.Id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Lấy giá trị TotalUnread từ JSON
                if (root.TryGetProperty("TotalUnread", out var totalUnreadElement))
                {
                    rs.TotalPage = totalUnreadElement.GetInt32();
                }
            }

            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }


        public async Task<ResponseClass> ListNotification(NotificationRequest request, bool isStaff)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var search = new NotificationSearch
            {
                PageIndex = request.PageIndex,
                PageSize = 20,
                Type = request.Type,
            };
            var query = _unitOfWork.Repository<Notification>().GetQueryable().Where(x =>
                x.AccountId == currentAccount.Id
                && (search.Type < 1 || search.Type == null || search.Type == x.Type)
                && x.IsStaff == isStaff
                && (isStaff == false || x.Title == "Đơn hàng mới")
            );
            var notifications = await query.Skip(search.PageIndex * search.PageSize).Take(search.PageSize).OrderByDescending(x => x.Id).ToListAsync();
            var total = await query.CountAsync();
            var pagedData = new PagedList<Notification>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = notifications
            };
            rs.TotalItem = total;
            rs.TotalPage = pagedData.TotalPage;
            var data = new List<NotificationResponse>();
            foreach (var item in notifications)
            {
                data.Add(new NotificationResponse
                {
                    ID = item.Id,
                    CreatedDate = item.Created,
                    Message = item.Content,
                    Status = item.IsRead ? 2 : 1,
                    Type = item.Type,
                    Title = item.Title,
                });
            }
            rs.data = data;
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> TransOrderDetail(TrackingRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode.Trim());
            if (transportation == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                return rs;
            }
            var transporationProducts = await _unitOfWork.Repository<TransportationProduct>().GetQueryable().Where(x => x.TransportationId == transportation.Id && transportation.ShipId == 5).ToListAsync();
            var transportFeeResponse = new TransportFeeResponse
            {
                ShipId = transportation.ShipId ?? 1,
                ProductName = "",
                TotalShip = 0,
                Weight = transportation.Weight ?? 0,
                Volume = transportation.Volume ?? 0,
                ShipVn = (double)(transportation.UnitWeight ?? 0),
                ShipVnVolume = (double)(transportation.UnitVolume ?? 0),
                Number = transportation.Quantity ?? 1,
                ShipInVn = 0,
                UnitPrice = 0,
                TotalPrice = (double)transportation.TotalPriceVND,
                Note = transportation.UserNote,
                UserUploadImage = transportation.UserUploadImage,
                TransportationProducts = transporationProducts,
            };
            var tracking = await _unitOfWork.Repository<Tracking>().GetQueryable().SingleOrDefaultAsync(x => x.TransportationId == transportation.Id);
            if (tracking != null)
            {
                transportFeeResponse.ProductName = tracking.ProductName;
                transportFeeResponse.TotalShip = Convert.ToDouble(tracking.TotalPrice);
                transportFeeResponse.ShipInVn = Convert.ToDouble(tracking.FeeShipVN);
                transportFeeResponse.UnitPrice = Convert.ToDouble(tracking.UnitPriceCYN);
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = transportFeeResponse;
            return rs;
        }

        public async Task<ResponseClass> CreateTracking(CreateTrackingAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var transportFeeResponse = await _trackingService.CreateForAPI(new CreateTrackingRequest
            {
                Barcode = request.Barcode,
                ProductName = request.ProductName,
                Note = request.Note,
                Number = request.Number,
                ShipInVn = request.ShipInVn,
                TotalShip = request.TotalShip,
                TotalVolume = request.TotalVolume,
                TotalWeight = request.TotalWeight,
                Warehouse = request.Warehouse
            }, currentAccount);
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = transportFeeResponse;
            return rs;
        }
        public async Task<ResponseClass> ContactConfig()
        {
            var rs = new ResponseClass();
            var config = await _unitOfWork.Repository<WebConfiguration>().GetQueryable().FirstOrDefaultAsync();
            var infor = new ContactConfigResponse
            {
                HotLine = config.Hotline,
                InsurancePercent = config.IsShowSignUpApp.ToString(),
                ZaloLink = config.ZaloLink,
                News1 = config.AppNotiImage
            };
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = infor;
            return rs;
        }
        public async Task<ResponseClass> UploadFile(IFormFile request)
        {
            var rs = new ResponseClass();
            var fileUrl = await _uploadFileService.UploadFile(request);
            rs.Message = fileUrl;
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> UpdateAccountInfo(UpdateAccountInfoRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var currentDate = DateTime.Now;
            if (!string.IsNullOrEmpty(request.Password))
            {
                string passwordEncryptValue = RandomStringWithText(16);
                string passwordEncryptKey = request.Password.Insert(2, passwordEncryptValue);
                var passwordHash = EncryptPassword(passwordEncryptKey, request.Password);
                currentAccount.PasswordHash = passwordHash;
                currentAccount.PasswordEncryptValue = passwordEncryptValue;
            }
            if (!string.IsNullOrEmpty(request.Avatar))
            {
                currentAccount.Avatar = request.Avatar;
            }
            currentAccount.Address = request.Address;
            currentAccount.FullName = request.FirstName;
            _unitOfWork.Repository<Account>().Update(currentAccount, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> AccountInfo(BaseAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var warehouseFrom = await _unitOfWork.Repository<Warehouse>().GetQueryable().FirstOrDefaultAsync(x => x.Type == (int)EWarehouseType.Reciever && x.Status == (int)EWarehouseStatus.Active);
            var warehouseTo = await _unitOfWork.Repository<Warehouse>().GetQueryable().FirstOrDefaultAsync(x => x.Type == (int)EWarehouseType.Destination && x.Status == (int)EWarehouseStatus.Active);
            var ship = await _unitOfWork.Repository<Warehouse>().GetQueryable().FirstOrDefaultAsync(x => x.Type == (int)EWarehouseType.Shipping && x.Status == (int)EWarehouseStatus.Active);
            var profileResponse = new ProfileResponse()
            {
                Username = currentAccount.Username,
                FirstName = currentAccount.FullName,
                LastName = "",
                Gender = 1,
                Dob = string.Format("{0:dd/MM/yyyy}", DateTime.Now),
                FromWarehouseID = warehouseFrom.Id,
                ToWarehouseID = warehouseTo.Id,
                ShipType = ship.Id,
                Address = currentAccount.Address,
                Avatar = currentAccount.Avatar ?? "",
                Email = currentAccount.Email,
                Phone = currentAccount.Phone,
                Wallet = string.Format("{0:N0}", 0)
            };
            rs.data = profileResponse;
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> CancelOrder(HandleIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            await _transportationService.CancelForAPI(request.ID, currentAccount);
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> Tracking(TrackingRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode.Trim());
            if (transportation == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                return rs;
            }
            var trackingResponse = new TrackingAppResponse
            {
                Barcode = transportation.Barcode,
                ID = transportation.Id,
                Status = transportation.Status,
                DateTQ = FormatDate.FormatNullDate(transportation.DateArrivedAtTQWarehouse),
                DateComingVN = FormatDate.FormatNullDate(transportation.DateExitedFromTQWarehouse),
                DateCheck = FormatDate.FormatNullDate(transportation.DateCustomsInspectedGoods),
                DateProcessVN = FormatDate.FormatNullDate(transportation.DateReturningToVNWarehouse),
                DateVN = FormatDate.FormatNullDate(transportation.DateArrivedAtVNWarehouse),
                DateExport = FormatDate.FormatNullDate(transportation.DateCompleted),
            };
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = trackingResponse;
            return rs;
        }
        public async Task<ResponseClass> CreateTransportation(CreateTransportationAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var items = new List<CreateTransportationItem>();
            foreach (var item in request.TransportationRequests)
            {
                items.Add(new CreateTransportationItem
                {
                    Barcode = item.Barcode,
                    Note = item.Note,
                    VoucherId = item.VoucherID ?? 0,
                    ImageUrl = item.ImageUrl
                });
            }
            var createTransportationRequest = new CreateTransportationRequest
            {
                TransportMethod = request.ShipType ?? 0,
                WarehouseFrom = request.FromWarehouse ?? 0,
                WarehouseTo = request.VnWarehouse ?? 0,
                Items = items,
            };
            try
            {
                await _transportationService.CreateForApi(createTransportationRequest, currentAccount);
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
                rs.Message = "Tạo đơn thành công";
                return rs;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("tồn tại"))
                {
                    rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                    rs.Status = APIUtils.ResponseMessage.Success.ToString();
                    rs.Message = ex.Message;
                    return rs;
                }
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Tạo thất bại";
                return rs;
            }
            
            
        }
        public async Task<ResponseClass> CreateSpeicalShip(CreateSpecialAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var items = new List<CreateTransportationItem>();
            foreach (var item in request.Items)
            {
                items.Add(new CreateTransportationItem
                {
                    Barcode = item.Barcode,
                    Note = item.Note,
                    VoucherId = item.VoucherId,
                    UserUploadQuantity = item.UserUploadQuantity,
                    UserUploadVolume = item.UserUploadVolume,
                    UserUploadWeight = item.UserUploadWeight,
                    Products = item.Products
                });
            }
            var createTransportationRequest = new CreateTransportationRequest
            {
                TransportMethod = 5,
                WarehouseFrom = request.FromWarehouse ?? 0,
                WarehouseTo = request.VnWarehouse ?? 0,
                Items = items,
            };
            if (await _transportationService.CreateForApi(createTransportationRequest, currentAccount))
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
                rs.Message = "Tạo đơn thành công";
                return rs;
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
            rs.Status = APIUtils.ResponseMessage.Error.ToString();
            rs.Message = "Tạo thất bại";
            return rs;
        }

        public async Task<ResponseClass> UpdateTransportationProduct(UpdateTransportationProductAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            var transportationProduct = await _unitOfWork.Repository<TransportationProduct>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.Id);
            if (transportationProduct == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "Không tìm thấy sản phẩm";
                return rs;
            }
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportationProduct.TransportationId && x.Status <= (int)ETransportationStatus.ArrivedAtTQWarehouse);
            if (transportation == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "Đơn không thể sửa";
                return rs;
            }

            transportationProduct.Name = request.Name;
            transportationProduct.Quantity = request.Quantity;
            transportationProduct.Dimensions = request.Dimensions;
            transportationProduct.Image = request.ImageUrl;
            transportationProduct.OtherInfor = request.OtherInfor;
            _unitOfWork.Repository<TransportationProduct>().Update(transportationProduct, DateTime.Now, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.Message = "Cập nhật thành công";
            return rs;
        }

        public async Task<ResponseClass> WarehouseShipType()
        {
            var rs = new ResponseClass();
            var warehouses = await _warehouseService.GetWarehousesByStatus((int)EWarehouseStatus.Active);
            var data = new WarehouseShipTypeResponse
            {
                FromWarehouses = warehouses.Where(x => x.Type == (int)EWarehouseType.Reciever).Select(x => new WarehouseResponse { ID = x.Id, Name = x.Name }).ToList(),
                VNWarehouses = warehouses.Where(x => x.Type == (int)EWarehouseType.Destination).Select(x => new WarehouseResponse { ID = x.Id, Name = x.Name }).ToList(),
                ShipTypes = warehouses.Where(x => x.Type == (int)EWarehouseType.Shipping).Select(x => new WarehouseResponse { ID = x.Id, Name = x.Name }).ToList(),
            };
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = data;
            return rs;
        }
        public async Task<ResponseClass> ListTransportation(ListTransportationRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            int PageSize = 20;
            var listTransportationResponse = new List<TransportationAppResponse>();
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(request.FD))
            {
                fromDate = DateTime.ParseExact(request.FD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(request.TD))
            {
                toDate = DateTime.ParseExact(request.TD, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            var searchRequest = new TransportationSearch
            {
                Barcode = string.IsNullOrEmpty(request.Code.Trim()) ? null : request.Code,
                Status = request.Status > -1 ? request.Status : null,
                FromDate = fromDate,
                ToDate = toDate,
                PageIndex = request.PageIndex + 1,
                PageSize = PageSize,
            };
            var loggedModel = new LoggedModel
            {
                Id = currentAccount.Id,
                IsStaff = 0,
                RoleId = currentAccount.RoleId,
            };
            var pagedData = await _transportationService.GetPagingForAPI(searchRequest, loggedModel);
            if (pagedData.TotalItem > 0)
            {
                foreach (var item in pagedData.Items)
                {
                    var trans = new TransportationAppResponse();

                    string exportRequestNote = $"Ngày tạo: {string.Format("{0:dd/MM/yyyy}", item.Created)}";

                    trans.ID = item.Id;
                    trans.Barcode = item.Barcode;

                    switch (item.Status)
                    {
                        case (int)ETransportationStatus.ArrivedAtTQWarehouse:
                            exportRequestNote = $"Ngày về kho TQ: {string.Format("{0:dd/MM/yyyy}", item.DateArrivedAtTQWarehouse)}";
                            break;
                        case (int)ETransportationStatus.ExitedFromTQWarehouse:
                            exportRequestNote = $"Ngày đang về kho VN: {string.Format("{0:dd/MM/yyyy}", item.DateExitedFromTQWarehouse)}";
                            break;
                        case (int)ETransportationStatus.CustomsInspectedGoods:
                            exportRequestNote = $"Ngày Hải quan kiểm hoá: {string.Format("{0:dd/MM/yyyy}", item.DateCustomsInspectedGoods)}";
                            break;
                        case (int)ETransportationStatus.ReturningToVNWarehouse:
                            exportRequestNote = $"Ngày Đang nhập khẩu: {string.Format("{0:dd/MM/yyyy}", item.DateReturningToVNWarehouse)}";
                            break;
                        case (int)ETransportationStatus.ArrivedAtVNWarehouse:
                            exportRequestNote = $"Ngày về VN: {string.Format("{0:dd/MM/yyyy}", item.DateArrivedAtVNWarehouse)}";
                            break;
                        case (int)ETransportationStatus.Completed:
                            exportRequestNote = $"Ngày xuất kho: {string.Format("{0:dd/MM/yyyy}", item.DateCompleted)}";
                            break;
                        default:
                            break;
                    }

                    trans.Weight = item.Weight ?? 0;
                    trans.Volume = item.Volume ?? 0;
                    trans.Quantity = item.Quantity ?? 1;
                    trans.Status = item.Status;

                    trans.StatusName = ETransportationStatusName.GetStatusName(item.Status);
                    trans.FromWarehouse = item.WarehouseFrom;
                    trans.VNWarehouse = item.WarehouseTo;
                    trans.ShippingType = item.ShipName;
                    trans.Note = item.UserNote;
                    trans.SensorFeeeVND = string.Format("{0:N0}", item.SurchargeVND);
                    trans.CreatedDate = string.Format("{0:dd/MM/yyyy}", item.Created);
                    string ngayxk = "";
                    if (item.DateCompleted != null)
                    {
                        ngayxk = string.Format("{0:dd/MM/yyyy}", item.DateCompleted);
                    }
                    trans.DateExport = ngayxk;

                    trans.ExportRequestNote = exportRequestNote;
                    trans.Warning = $"Voucher: {string.Format("{0:N0}", item.Discount)} VND";
                    listTransportationResponse.Add(trans);
                }
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.data = listTransportationResponse;
            rs.TotalPage = pagedData.TotalPage;
            rs.TotalItem = pagedData.TotalItem;
            return rs;
        }

        public async Task<ResponseClass> UpdateDeviceToken(UpdateOneSignalIdRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            currentAccount.OneSignalId = request.Device;
            _unitOfWork.Repository<Account>().Update(currentAccount, DateTime.Now, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }

        public async Task<ResponseClass> LogOut(BaseAppRequest request)
        {
            var rs = new ResponseClass();
            var account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Id == request.UID);
            if (account == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Lỗi hệ thống";
                return rs;
            }
            account.OneSignalId = "";
            account.AppToken = "";
            _unitOfWork.Repository<Account>().Update(account, DateTime.Now, 0);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }
        public async Task<ResponseClass> ForgotPassword(ForgotPasswordRequest request)
        {
            var rs = new ResponseClass();
            var account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Email == request.Email.Trim());
            if (account == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Email không tồn tại trong hệ thống.";
                return rs;
            }
            var newPassword = GenerateRandomPassword();
            string passwordEncryptValue = RandomStringWithText(16);
            string passwordEncryptKey = newPassword.Insert(2, passwordEncryptValue);
            var passwordHash = EncryptPassword(passwordEncryptKey, newPassword);
            account.PasswordHash = passwordHash;
            account.PasswordEncryptValue = passwordEncryptValue;
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Repository<Account>().Update(account, DateTime.Now, 0);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                _sendEmailService.Send(request.Email, "Mật khẩu mới", $"Mật khẩu tài khoản {account.Username}: {newPassword}");
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
                rs.Status = APIUtils.ResponseMessage.Success.ToString();
                rs.Message = "Hệ thống đã gửi 1 email mới cho bạn, vui lòng kiểm tra email và thiết lập lại mật khẩu.";
                return rs;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Lỗi gửi mail.";
                return rs;
            }
        }

        public async Task<ResponseClass> CheckLogin(BaseAppRequest request)
        {
            var rs = new ResponseClass();
            var currentAccount = await GetSessionAsync(request);
            if (currentAccount == null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Logout = "1";
                return rs;
            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;
        }

        public async Task<ResponseClass> Register(SignUpRequest request)
        {
            var rs = new ResponseClass();
            Log.Information($"Sign Up: {request.UserName}; {request.Password};");
            var account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Username == request.UserName);
            if (account != null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Username đã tồn tại";
                return rs;
            }
            account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Email == request.Email);
            if (account != null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Email đã tồn tại";
                return rs;
            }
            account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Phone == request.Phone);
            if (account != null)
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Số điện thoại đã tồn tại";
                return rs;
            }
            string passwordEncryptValue = RandomStringWithText(16);
            string passwordEncryptKey = request.Password.Insert(2, passwordEncryptValue);
            var passwordHash = EncryptPassword(passwordEncryptKey, request.Password);
            var newAccount = _mapper.Map<Account>(request);
            newAccount.PasswordHash = passwordHash;
            newAccount.RoleId = (int)ERoleId.User;
            newAccount.PasswordEncryptValue = passwordEncryptValue;
            newAccount.PostOffice = PostOfficeName.GetPostOffice().FirstOrDefault();

            try
            {
                await _unitOfWork.Repository<Account>().Add(newAccount, DateTime.Now, 0);
                await _unitOfWork.SaveAsync();
                var notification = new Notification
                {
                    Title = "Khách hàng mới",
                    Content = $"Khách hàng mới đăng ký Username: {newAccount.Username}",
                    WebUrl = "/customer",
                    Type = (int)ENotificationType.NewCustomer,
                    IsStaff = true,
                };
                await _notificationService.SendNotification(notification, DateTime.Now, 0, staffRoleIds: new List<int> { (int)ERoleId.Admin, (int)ERoleId.Manager, });
            }
            catch
            {

            }
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            return rs;

        }

        public async Task<ResponseClass> LoginStaff(SignInRequest request)
        {
            var rs = new ResponseClass();
            Log.Information($"Sign In App: {request.Username}; {request.Password};");
            var account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Username == request.Username && x.RoleId != (int)ERoleId.User);
            if (account == null || account?.PasswordHash != EncryptPassword(request.Password.Insert(2, account?.PasswordEncryptValue), request.Password))
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Sai Username hoặc Password, vui lòng kiểm tra lại.";
                return rs;
            }
            string accessToken = RandomStringWithText(20);
            account.AppType = request.Type;
            account.AppTypeName = request.TypeName;
            account.AppToken = accessToken;
            account.OneSignalId = request.DeviceToken;
            _unitOfWork.Repository<Account>().Update(account, DateTime.Now, account.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            int postOfficeInt = 0;
            if (account.RoleId == (int)ERoleId.VNWarehouseStaff)
            {
                postOfficeInt = Array.IndexOf(PostOfficeName.GetPostOffice().ToArray(), account.PostOffice) + 1;
            }

            rs.Account = new AppAccountResponse
            {
                ID = account.Id,
                Role = account.RoleId,
                Username = account.Username,
                PostOffice = postOfficeInt,
                WarehouseTypeTQ = account.TransportationType
            };
            rs.Key = accessToken;
            return rs;
        }

        public async Task<ResponseClass> Login(SignInRequest request)
        {
            var rs = new ResponseClass();
            Log.Information($"Sign In App: {request.Username}; {request.Password};");
            var account = await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Username == request.Username);
            if (account == null || account?.PasswordHash != EncryptPassword(request.Password.Insert(2, account?.PasswordEncryptValue), request.Password))
            {
                rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.NotFound);
                rs.Status = APIUtils.ResponseMessage.Error.ToString();
                rs.Message = "Sai Username hoặc Password, vui lòng kiểm tra lại.";
                return rs;
            }
            string accessToken = RandomStringWithText(20);
            account.AppType = request.Type;
            account.AppTypeName = request.TypeName;
            account.AppToken = accessToken;
            account.OneSignalId = request.DeviceToken;
            _unitOfWork.Repository<Account>().Update(account, DateTime.Now, account.Id);
            await _unitOfWork.SaveAsync();
            rs.Code = APIUtils.GetResponseCode(APIUtils.ResponseCode.SUCCESS);
            rs.Status = APIUtils.ResponseMessage.Success.ToString();
            rs.Account = new AppAccountResponse
            {
                ID = account.Id,
                Role = account.RoleId,
                Username = account.Username,
                SpecialShipId = account.SpecialShipId,
            };
            rs.Key = accessToken;
            return rs;
        }

        private async Task<Account> GetSessionAsync(BaseAppRequest request)
        {
            return await _unitOfWork.Repository<Account>().GetQueryable().FirstOrDefaultAsync(x => x.Id == request.UID && x.AppToken == request.Key);
        }
        private string RandomStringWithText(int numberrandom)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[numberrandom];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
        private string GenerateJwtToken(Account account)
        {
            var claims = new List<Claim>
            {
                new Claim("Username", account.Username),
                new Claim("Id", account.Id.ToString()),
                new Claim("Role", account.RoleId.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string EncryptPassword(string key, string data)
        {
            data = data.Trim();
            byte[] keydata = Encoding.ASCII.GetBytes(key);
            string md5String = BitConverter.ToString(new
            MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();
            byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));
            TripleDES tripdes = TripleDESCryptoServiceProvider.Create();
            tripdes.Mode = CipherMode.ECB;
            tripdes.Key = tripleDesKey;
            tripdes.GenerateIV();
            MemoryStream ms = new MemoryStream();
            CryptoStream encStream = new CryptoStream(ms, tripdes.CreateEncryptor(),
            CryptoStreamMode.Write);
            encStream.Write(Encoding.ASCII.GetBytes(data), 0,
            Encoding.ASCII.GetByteCount(data));
            encStream.FlushFinalBlock();
            byte[] cryptoByte = ms.ToArray();
            ms.Close();
            encStream.Close();
            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0)).Trim();
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


    }

}
