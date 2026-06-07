using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextService _httpContextService;
        private readonly IUploadFileService _uploadFileService;

        public TrackingService(IUnitOfWork unitOfWork, IHttpContextService httpContextService, IUploadFileService uploadFileService)
        {
            _unitOfWork = unitOfWork;
            _httpContextService = httpContextService;
            _uploadFileService = uploadFileService;
        }

        public async Task<bool> UpdateTransportationProduct(UpdateTransportationProductRequest request)
        {
            var transportationProduct = await _unitOfWork.Repository<TransportationProduct>().GetQueryable().SingleOrDefaultAsync(x => x.Id == request.Id);
            if (transportationProduct == null)
            {
                throw new AppException("Không tìm thấy sản phẩm");
            }
            var currentDate = DateTime.Now;
            var currentAccount = await _httpContextService.GetCurrentAccount();
            if (request.ImageFile != null)
            {
                transportationProduct.Image = await _uploadFileService.UploadFile(request.ImageFile);
            }
            transportationProduct.Name = request.Name;
            transportationProduct.Dimensions = request.Dimensions;
            transportationProduct.Quantity = request.Quantity;
            transportationProduct.OtherInfor = request.OtherInfor;
            _unitOfWork.Repository<TransportationProduct>().Update(transportationProduct, currentDate, currentAccount.Id);
            await _unitOfWork.SaveAsync();
            return true;
        }
        public async Task<TrackingResponse> TrackingByBarcode(string barcode)

        {
            var trackingResponse = await (from transportation in _unitOfWork.Repository<Transportation>().GetQueryable()
                                          join account in _unitOfWork.Repository<Account>().GetQueryable() on transportation.AccountId equals account.Id into accountJoin
                                          from account in accountJoin.DefaultIfEmpty()
                                          join warehouseFrom in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Reciever) on transportation.FromId equals warehouseFrom.Id into warehouseFromJoin
                                          from warehouseFrom in warehouseFromJoin.DefaultIfEmpty()
                                          join warehouseTo in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Destination) on transportation.ToId equals warehouseTo.Id into warehouseToJoin
                                          from warehouseTo in warehouseToJoin.DefaultIfEmpty()
                                          join shippingType in _unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => x.Type == (int)EWarehouseType.Shipping) on transportation.ShipId equals shippingType.Id into shippingTypeJoin
                                          from shippingType in shippingTypeJoin.DefaultIfEmpty()
                                          where transportation.Status != (int)ETransportationStatus.Cancel
                                                 && transportation.Barcode == barcode.Trim()
                                          select new TrackingResponse
                                          {
                                              Id = transportation.Id,
                                              Barcode = transportation.Barcode,
                                              Status = transportation.Status,
                                              Weight = transportation.Weight,
                                              Volume = transportation.Volume,
                                              Quantity = transportation.Quantity,
                                              WarehouseFrom = warehouseFrom != null ? warehouseFrom.Name : string.Empty,
                                              WarehouseTo = warehouseTo != null ? warehouseTo.Name : string.Empty,
                                              ShipName = shippingType != null ? shippingType.Name : string.Empty,
                                              ShipId = shippingType != null ? shippingType.Id : 0,
                                              Note = transportation.UserNote,
                                              Created = transportation.Created,
                                              DateArrivedAtTQWarehouse = transportation.DateArrivedAtTQWarehouse,
                                              DateArrivedAtVNWarehouse = transportation.DateArrivedAtVNWarehouse,
                                              DateCompleted = transportation.DateCompleted,
                                              DateCustomsInspectedGoods = transportation.DateCustomsInspectedGoods,
                                              DateExitedFromTQWarehouse = transportation.DateExitedFromTQWarehouse,
                                              DateReturningToVNWarehouse = transportation.DateReturningToVNWarehouse,

                                              UserUploadWeight = transportation.UserUploadWeight,
                                              UserUploadVolume = transportation.UserUploadVolume,
                                              UserUploadQuantity = transportation.UserUploadQuantity,
                                              UserUploadImage = transportation.UserUploadImage,
                                          }).SingleOrDefaultAsync();
            if (trackingResponse == null)
                return null;
            var transportationProducts = await _unitOfWork.Repository<TransportationProduct>().GetQueryable().Where(x => x.TransportationId == trackingResponse.Id).ToListAsync();
            if (transportationProducts.Any())
            {
                trackingResponse.Products = transportationProducts;
            }
            return trackingResponse;
        }
        public async Task<TransportFeeResponse> Create(CreateTrackingRequest request)
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            return await CreateForAPI(request, currentAccount);
        }
        public async Task<TransportFeeResponse> CreateForAPI(CreateTrackingRequest request, Account currentAccount)
        {
            var config = await _unitOfWork.Repository<WebConfiguration>().GetQueryable().SingleOrDefaultAsync();
            var priceList = await _unitOfWork.Repository<Pricing>().GetQueryable().Where(x => x.ToWarehouseId == request.Warehouse).ToListAsync();
            var weightPricing = priceList.Where(x => x.Type == (int)EPricingType.Weight && request.TotalWeight >= x.RangeMin && request.TotalWeight < x.RangeMax).FirstOrDefault();
            decimal unitWeight = 0, unitVolume = 0;
            if (weightPricing == null)
            {
                unitWeight = 0;
            }
            else
            {
                unitWeight = weightPricing.PricePerUnit;
            }
            var volumePricing = priceList.Where(x => x.Type == (int)EPricingType.Volume && request.TotalVolume >= x.RangeMin && request.TotalVolume < x.RangeMax).FirstOrDefault();
            if (volumePricing == null)
            {
                unitVolume = 0;
            }
            else
            {
                unitVolume = volumePricing.PricePerUnit;
            }

            decimal? priceWeight = unitWeight * (decimal)request.TotalWeight;
            decimal? priceVolume = unitVolume * (decimal)request.TotalVolume;
            decimal? priceTQVNFN = priceWeight > priceVolume ? priceWeight : priceVolume;
            decimal unitPrice = (request.TotalShip ?? 0) / (request.Number ?? 1);
            decimal? totalPriceVND = (request.TotalShip + request.ShipInVn) * config.Currency + priceTQVNFN / (request.Number ?? 1);

            var data = new TransportFeeResponse()
            {
                ProductName = request.ProductName,
                TotalShip = Convert.ToDouble(request.TotalShip),
                Weight = request.TotalWeight ?? 0,
                Volume = request.TotalVolume ?? 0,
                ShipVn = (double)priceWeight,
                ShipVnVolume = (double)priceVolume,
                Number = request.Number ?? 1,
                ShipInVn = Convert.ToDouble(request.ShipInVn),
                UnitPrice = Convert.ToDouble(unitPrice),
                TotalPrice = Convert.ToDouble(totalPriceVND),
                Note = request.Note
            };
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == request.Barcode.Trim());
            if (transportation == null)
            {
                return data;
            }
            var tracking = await _unitOfWork.Repository<Tracking>().GetQueryable().SingleOrDefaultAsync(x => x.TransportationId == transportation.Id);
            if (tracking == null)
            {
                tracking = new Tracking();
                tracking.ProductName = request.ProductName;
                tracking.TotalPrice = request.TotalShip;
                tracking.Weight = (decimal)request.TotalWeight;
                tracking.FeeShipTQ = priceWeight;
                tracking.Quantity = request.Number;
                tracking.FeeShipVN = request.ShipInVn;
                tracking.UnitPriceCYN = unitPrice;
                tracking.UnitPriceVND = totalPriceVND;
                tracking.TransportationId = transportation.Id;
                tracking.Note = request.Note;
                tracking.VNWarehouseId = request.Warehouse;
                tracking.Volume = (decimal)request.TotalVolume;
                tracking.FeeShipVNVolume = priceVolume;
                await _unitOfWork.Repository<Tracking>().Add(tracking, DateTime.Now, currentAccount.Id);
            }
            else
            {
                tracking.ProductName = request.ProductName;
                tracking.TotalPrice = request.TotalShip;
                tracking.Weight = (decimal)request.TotalWeight;
                tracking.FeeShipTQ = priceWeight;
                tracking.Quantity = request.Number;
                tracking.FeeShipVN = request.ShipInVn;
                tracking.UnitPriceCYN = unitPrice;
                tracking.UnitPriceVND = totalPriceVND;
                tracking.Note = request.Note;
                tracking.VNWarehouseId = request.Warehouse;
                tracking.Volume = (decimal)request.TotalVolume;
                tracking.FeeShipVNVolume = priceVolume;
                _unitOfWork.Repository<Tracking>().Update(tracking, DateTime.Now, currentAccount.Id);
            }
            await _unitOfWork.SaveAsync();
            return data;
        }

        public async Task<TransportFeeResponse> GetByBarcode(string barcode)
        {
            var transportFeeResponse = new TransportFeeResponse();
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == barcode.Trim());
            if (transportation == null)
            {
                return transportFeeResponse;
            }
            var tracking = await _unitOfWork.Repository<Tracking>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.Id);
            if (tracking != null)
            {
                transportFeeResponse.ProductName = tracking.ProductName;
                transportFeeResponse.TotalShip = (double)(tracking.TotalPrice ?? 0);
                transportFeeResponse.Weight = (double)(tracking.Weight ?? 0);
                transportFeeResponse.Volume = (double)(tracking.Volume ?? 0);
                transportFeeResponse.ShipVn = (double)(tracking.FeeShipTQ ?? 0);
                transportFeeResponse.ShipVnVolume = (double)(tracking.FeeShipVNVolume ?? 0);
                transportFeeResponse.Number = tracking.Quantity ?? 1;
                transportFeeResponse.ShipInVn = (double)(tracking.FeeShipVN ?? 0);
                transportFeeResponse.UnitPrice = (double)(tracking.UnitPriceCYN ?? 0);
                transportFeeResponse.TotalPrice = (double)(tracking.TotalPrice ?? 0);
                transportFeeResponse.Note = tracking.Note;
            }
            else
            {
                transportFeeResponse.Weight = (double)(transportation.Weight ?? 0);
                transportFeeResponse.Volume = (double)(transportation.Volume ?? 0);
                transportFeeResponse.ShipVn = (double)(transportation.UnitWeight ?? 0);
                transportFeeResponse.ShipVnVolume = (double)(transportation.UnitVolume ?? 0);
                transportFeeResponse.Number = transportation.Quantity ?? 1;
                transportFeeResponse.TotalPrice = (double)transportation.TotalPriceVND;
                transportFeeResponse.Note = transportation.UserNote;
            }
            return transportFeeResponse;
        }

        public async Task<string> GetInfo(string barcode)
        {
            var transportation = await _unitOfWork.Repository<Transportation>().GetQueryable().SingleOrDefaultAsync(x => x.Barcode == barcode.Trim());
            if (transportation == null || transportation?.Status == (int)ETransportationStatus.New)
            {
                return $"https://www.baidu.com/s?ie=utf-8&f=8&rsv_bp=1&rsv_idx=1&ch=&tn=baidu&bar=&wd={barcode.Trim()}&rn=&fenlei=256&oq=&rsv_pq=0xca354ba9070f5608&rsv_t=4787ooTMoXXAxx0oax2j85mAnyOPfKYAiMlt5fmn%2FEOMOUtXnaMEyJ3vEdfN&rqlang=en";
            }
            var customer = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == transportation.AccountId);
            string result = "";
            result += $@"<div class=""content1"" style=""width: 75%; margin-left: 11%"">
                          <aside style=""text-align: left"" class=""side trk-info fr"">
                            <table>
                              <tbody>
                                <tr>
                                  <th style=""width: 50%"">Username:</th>
                                  <td class=""m-color"">{customer.Username}</td>
                                </tr>
                                <tr>
                                  <th style=""width: 50%"">Số điện thoại:</th>
                                  <td class=""m-color"">{customer.Phone}</td>
                                </tr>
                                <tr>
                                  <th style=""width: 50%"">Địa chỉ:</th>
                                  <td class=""m-color"">
                                    {customer.Address}
                                  </td>
                                </tr>
                                <tr>
                                  <th style=""width: 50%"">Mã vận đơn:</th>
                                  <td class=""m-color"">{transportation.Barcode}</td>
                                </tr>
                                <tr>
                                  <th style=""width: 50%"">ID đơn hàng:</th>
                                  <td class=""m-color"">{transportation.Id}</td>
                                </tr>
                                <tr>
                                  <th style=""width: 50%"">Loại đơn hàng:</th>
                                  <td class=""m-color"">Đơn hàng vận chuyển hộ</td>
                                </tr>
                              </tbody>
                            </table>
                          </aside>";
            result += $@"<aside class=""side trk-history fl"">
                           <ul class=""list"">";
            if (transportation.DateArrivedAtTQWarehouse.HasValue)
            {
                result += $@"<li class=""it clear"">
                                <div class=""date-time tq grey89""><p>{FormatDate.FormatNullDate(transportation.DateArrivedAtTQWarehouse)}</p></div>
                                <div class=""statuss ok"">
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">Trạng thái:</span
                                    ><span class=""m-color""> {ETransportationStatusName.GetStatusName((int)ETransportationStatus.ArrivedAtTQWarehouse)} </span>
                                    </p>
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">NV Xử lý:</span>
                                    <span class=""m-color"">{transportation.AccountArrivedAtTQWarehouse}</span>
                                    </p>
                                </div>
                            </li>";
            }
            if (transportation.DateExitedFromTQWarehouse.HasValue)
            {
                result += $@"<li class=""it clear"">
                                <div class=""date-time tq grey89""><p>{FormatDate.FormatNullDate(transportation.DateExitedFromTQWarehouse)}</p></div>
                                <div class=""statuss ok"">
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">Trạng thái:</span
                                    ><span class=""m-color""> {ETransportationStatusName.GetStatusName((int)ETransportationStatus.ExitedFromTQWarehouse)} </span>
                                    </p>
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">NV Xử lý:</span>
                                    <span class=""m-color"">{transportation.AccountExitedFromTQWarehouse}</span>
                                    </p>
                                </div>
                            </li>";
            }
            if (transportation.DateCustomsInspectedGoods.HasValue)
            {
                result += $@"<li class=""it clear"">
                                <div class=""date-time tq grey89""><p>{FormatDate.FormatNullDate(transportation.DateCustomsInspectedGoods)}</p></div>
                                <div class=""statuss ok"">
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">Trạng thái:</span
                                    ><span class=""m-color""> {ETransportationStatusName.GetStatusName((int)ETransportationStatus.CustomsInspectedGoods)} </span>
                                    </p>
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">NV Xử lý:</span>
                                    <span class=""m-color"">{transportation.AccountCustomsInspectedGoods}</span>
                                    </p>
                                </div>
                            </li>";
            }
            if (transportation.DateReturningToVNWarehouse.HasValue)
            {
                result += $@"<li class=""it clear"">
                                <div class=""date-time tq grey89""><p>{FormatDate.FormatNullDate(transportation.DateReturningToVNWarehouse)}</p></div>
                                <div class=""statuss ok"">
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">Trạng thái:</span
                                    ><span class=""m-color""> {ETransportationStatusName.GetStatusName((int)ETransportationStatus.ReturningToVNWarehouse)} </span>
                                    </p>
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">NV Xử lý:</span>
                                    <span class=""m-color"">{transportation.AccountReturningToVNWarehouse}</span>
                                    </p>
                                </div>
                            </li>";
            }
            if (transportation.DateArrivedAtVNWarehouse.HasValue)
            {
                result += $@"<li class=""it clear"">
                                <div class=""date-time tq grey89""><p>{FormatDate.FormatNullDate(transportation.DateArrivedAtVNWarehouse)}</p></div>
                                <div class=""statuss ok"">
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">Trạng thái:</span
                                    ><span class=""m-color""> {ETransportationStatusName.GetStatusName((int)ETransportationStatus.ArrivedAtVNWarehouse)} </span>
                                    </p>
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">NV Xử lý:</span>
                                    <span class=""m-color"">{transportation.AccountArrivedAtVNWarehouse}</span>
                                    </p>
                                </div>
                            </li>";
            }
            if (transportation.DateCompleted.HasValue)
            {
                result += $@"<li class=""it clear"">
                                <div class=""date-time tq grey89""><p>{FormatDate.FormatNullDate(transportation.DateCompleted)}</p></div>
                                <div class=""statuss ok"">
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">Trạng thái:</span
                                    ><span class=""m-color""> {ETransportationStatusName.GetStatusName((int)ETransportationStatus.Completed)} </span>
                                    </p>
                                    <p class=""tit"">
                                    <span class=""grey89 font-w"">NV Xử lý:</span>
                                    <span class=""m-color"">{transportation.AccountCompleted}</span>
                                    </p>
                                </div>
                            </li>";
            }
            result += @"    </ul>
                        </aside>
                      </div>";
            return result;
        }
    }
}
