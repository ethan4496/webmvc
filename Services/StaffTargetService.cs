using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Responses;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Services
{
    public class StaffTargetService : IStaffTargetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;
        private readonly IUploadFileService _uploadFileService;

        public StaffTargetService(IUnitOfWork unitOfWork, IMapper mapper,
            IHttpContextService httpContextService, IUploadFileService uploadFileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextService = httpContextService;
            _uploadFileService = uploadFileService;
        }

        public async Task<List<StaffTargetResponse>> GetListStaffTarget(DateTime dataDate, int? staffId = null)
        {
            var currentAccount = _httpContextService.GetLoggedModel();

            if (staffId != currentAccount.Id && currentAccount.RoleId == (int)ERoleId.Sale)
            {
                throw new UnauthorizedAccessException("Không có quyền truy cập");
            }
            DateTime fromDate = new DateTime(dataDate.Year, dataDate.Month, 1);
            DateTime toDate = fromDate.AddMonths(1).AddDays(-1);
            var response = new List<StaffTargetResponse>();
            var checkTarget = await _unitOfWork.Repository<StaffTarget>().GetQueryable().Where(x => x.Year == dataDate.Year && x.Month == dataDate.Month).Select(x => x.AccountId).ToListAsync();
            var sales = await _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.RoleId == (int)ERoleId.Sale && !checkTarget.Contains(x.Id)).Select(x => x.Id).ToListAsync();

            if (sales.Count > 0)
            {
                var staffTargets = new List<StaffTarget>();
                foreach (var sale in sales)
                {
                    staffTargets.Add(new StaffTarget
                    {
                        AccountId = sale,
                        Month = dataDate.Month,
                        Year = dataDate.Year,
                        NewAccount = 0,
                        Order = 0,
                        Weight = 0,
                        Volume = 0,
                        TotalPriceVND = 0,
                        TotalPriceVNDCN = 0,
                        TotalPriceVNDHT = 0,
                        TotalPriceVNDDT = 0,
                    });
                }
                await _unitOfWork.Repository<StaffTarget>().AddRange(staffTargets, DateTime.Now, 1);
                await _unitOfWork.SaveAsync();
            }
            var staffTargetResponses = await (from staffTarget in _unitOfWork.Repository<StaffTarget>().GetQueryable()
                                              join sale in _unitOfWork.Repository<Account>().GetQueryable()
                                                    .Where(x => x.RoleId == (int)ERoleId.Sale)
                                              on staffTarget.AccountId equals sale.Id
                                              where staffTarget.Year == dataDate.Year
                                              && staffTarget.Month == dataDate.Month
                                              && (staffId == null || staffId == staffTarget.AccountId)
                                              select new StaffTargetResponse
                                              {
                                                  Id = staffTarget.Id,
                                                  AccountId = staffTarget.AccountId,
                                                  Username = sale.Username,
                                                  Order = staffTarget.Order,
                                                  NewAccount = staffTarget.NewAccount,
                                                  Weight = staffTarget.Weight,
                                                  Volume = staffTarget.Volume,
                                                  TotalPriceVND = staffTarget.TotalPriceVND,
                                                  TotalPriceVNDCN = staffTarget.TotalPriceVNDCN,
                                                  TotalPriceVNDHT = staffTarget.TotalPriceVNDHT,
                                                  TotalPriceVNDRealDT = staffTarget.TotalPriceVNDDT,
                                                  StaffAvatar = sale.StaffAvatar,
                                              }).ToListAsync();
            foreach (var staffTargetResponse in staffTargetResponses)
            {
                var transportationQuery = _unitOfWork.Repository<Transportation>().GetQueryable()
                    .Where(x => x.StaffId == staffTargetResponse.AccountId
                        && x.Status == (int)ETransportationStatus.Completed
                        && x.DateCompleted.HasValue
                        && x.DateCompleted.Value.Date >= fromDate
                        && x.DateCompleted.Value.Date <= toDate
                        );
                var orderReal = await transportationQuery.CountAsync();
                var weightReal = await transportationQuery.SumAsync(x => x.Weight);
                var volumeReal = await transportationQuery.SumAsync(x => x.Volume);
                var totalPriceVNDReal = await transportationQuery.SumAsync(x => x.TotalPriceVND);
                var totalPriceVNDRealCN = await transportationQuery.Where(x => x.ShipId == 5).SumAsync(x => x.TotalPriceVND);
                var totalPriceVNDRealHT = await transportationQuery.Where(x => x.ShipId == 4).SumAsync(x => x.TotalPriceVND);
                var totalPriceVNDRealDT = await transportationQuery.Where(x => x.ShipId == 3).SumAsync(x => x.TotalPriceVND);
                var customerQuery = _unitOfWork.Repository<Account>().GetQueryable()
                    .Where(x => x.SaleId == staffTargetResponse.AccountId);
                var totalCustomer = await customerQuery.CountAsync();
                var totalNewCustomer = await customerQuery.Where(x => x.Created.Date >= fromDate && x.Created.Date <= toDate).CountAsync();
                var customerIdInTransportations = await transportationQuery.Select(x => x.AccountId).Distinct().ToListAsync();
                var totalCustomerOrdered = await customerQuery.Where(x => customerIdInTransportations.Contains(x.Id)).CountAsync();
                var newCustomerOrderedIds = await customerQuery.Where(x => x.Created.Date >= fromDate && x.Created.Date <= toDate).Select(x => x.Id).Distinct().ToListAsync();
                var totalPriceVNDNewCustomerOrdered = await transportationQuery.Where(x => newCustomerOrderedIds.Contains(x.AccountId ?? 0)).SumAsync(x => x.TotalPriceVND);

                staffTargetResponse.OrderReal = orderReal;
                staffTargetResponse.WeightReal = Math.Round(weightReal ?? 0, 2);
                staffTargetResponse.VolumeReal = Math.Round(volumeReal ?? 0, 5);
                staffTargetResponse.NewAccountReal = totalNewCustomer;
                staffTargetResponse.TotalPriceVNDReal = totalPriceVNDReal;
                staffTargetResponse.TotalPriceVNDRealCN = totalPriceVNDRealCN;
                staffTargetResponse.TotalPriceVNDRealHT = totalPriceVNDRealHT;
                staffTargetResponse.TotalPriceVNDRealDT = totalPriceVNDRealDT;
                staffTargetResponse.TotalAccount = totalCustomer;
                staffTargetResponse.TotalNewAccountHasOrder = totalCustomerOrdered;
                staffTargetResponse.TotalPriceVNDNewAccountHasOrder = totalPriceVNDNewCustomerOrdered;

                double totalPrice = (double)totalPriceVNDReal; // Tổng giá trị để tính

                if (totalPrice > 120000000)
                {
                    double baseAmount = (double)(staffTargetResponse.TotalPriceVND ?? 0);
                    if (staffTargetResponse.AccountId == 16594)
                    {
                        staffTargetResponse.Profit = (totalPrice - 120000000) * 0.005;
                        staffTargetResponse.TotalPriceVND = 120000000;
                    }
                    else
                    {
                        double amount120To500 = Math.Min(totalPrice, 500000000) - Math.Max(baseAmount, 120000000);
                        double amount500To1000 = Math.Min(totalPrice, 1000000000) - 500000000;

                        // Nếu vượt 120 triệu thì bắt đầu tính phần từ 120 triệu đến 500 triệu
                        if (amount120To500 > 0)
                        {
                            staffTargetResponse.Profit += amount120To500 * 0.015; // Áp dụng 1.5% cho phần từ 120 đến 500 triệu
                        }

                        // Nếu vượt 500 triệu thì tính tiếp phần từ 500 triệu đến 1 tỷ
                        if (amount500To1000 > 0)
                        {
                            staffTargetResponse.Profit += amount500To1000 * 0.02; // Áp dụng 2% cho phần từ 500 triệu đến 1 tỷ
                        }

                        // Nếu vượt 1 tỷ thì tính tiếp với tỷ lệ phần trăm 3%
                        if (totalPrice > 1000000000)
                        {
                            staffTargetResponse.Profit += (totalPrice - 1000000000) * 0.03; // Áp dụng 3% cho phần vượt quá 1 tỷ
                        }
                    }
                }

                staffTargetResponse.Status = "<span class=\"badge bg-danger text-white p-2\">Chưa hoàn thành</span>";
                if (orderReal >= staffTargetResponse.Order && weightReal >= staffTargetResponse.Weight && volumeReal >= staffTargetResponse.Volume && totalNewCustomer >= staffTargetResponse.NewAccount && totalPriceVNDReal >= staffTargetResponse.TotalPriceVND)
                {
                    staffTargetResponse.Status = "<span class=\"badge bg-success text-white p-2\">Hoàn thành</span>";
                }
                response.Add(staffTargetResponse);
            }
            return response;
        }

        public async Task<bool> UpdateStaffImage(int id, IFormFile image)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var sale = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            if (sale == null)
                throw new AppException("Không tìm thấy nhân viên");
            sale.StaffAvatar = await _uploadFileService.UploadImage(image);
            _unitOfWork.Repository<Account>().Update(sale, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> UpdateStaffTarget(int id, StaffTarget request)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var staffTarget = await _unitOfWork.Repository<StaffTarget>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            if (staffTarget == null)
                throw new AppException("Không tìm thấy nhân viên");
            staffTarget.TotalPriceVND = request.TotalPriceVND;
            staffTarget.TotalPriceVNDCN = request.TotalPriceVNDCN;
            staffTarget.TotalPriceVNDHT = request.TotalPriceVNDHT;
            staffTarget.TotalPriceVNDDT = request.TotalPriceVNDDT;
            staffTarget.Volume = request.Volume;
            staffTarget.Weight = request.Weight;
            staffTarget.NewAccount = request.NewAccount;
            staffTarget.Order = request.Order;
            _unitOfWork.Repository<StaffTarget>().Update(staffTarget, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }
    }
}
