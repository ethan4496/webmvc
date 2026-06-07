using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextService _httpContextService;
        private readonly IUploadFileService _uploadFileService;

        public StatisticService(IUnitOfWork unitOfWork, IHttpContextService httpContextService,
            IUploadFileService uploadFileService)
        {
            _unitOfWork = unitOfWork;
            _httpContextService = httpContextService;
            _uploadFileService = uploadFileService;
        }

        public async Task<AccountHomeResponse> GetDashboard()
        {
            var loggedModel = await _httpContextService.GetCurrentAccount();
            // Lấy danh sách đơn hàng của tài khoản hiện tại
            var orders = _unitOfWork.Repository<Transportation>()
                .GetQueryable();
            if (loggedModel.RoleId == (int)ERoleId.Sale)
                orders = orders
                .Where(x => x.StaffId == loggedModel.Id);
            // Group theo trạng thái và đếm
            var counts = await orders
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Quantity = g.Count() })
                .ToListAsync();

            // Tạo list CountStatus cho tất cả trạng thái enum
            var countOrderList = Enum.GetValues(typeof(ETransportationStatus))
                .Cast<ETransportationStatus>()
                .Where(statusEnum => (int)statusEnum != (int)ETransportationStatus.Cancel)
                .Select(statusEnum =>
                {
                    var match = counts.FirstOrDefault(c => c.Status == (int)statusEnum);
                    return new CountStatus
                    {
                        Id = (int)statusEnum,
                        Quantity = match?.Quantity ?? 0
                    };
                })
                .OrderBy(x => x.Id)
                .ToList();

            var unPayOutOfStock = await (
                                        from o in _unitOfWork.Repository<OutOfStock>().GetQueryable()
                                        join a in _unitOfWork.Repository<Account>().GetQueryable() on o.AccountId equals a.Id into accJoin
                                        from acc in accJoin.DefaultIfEmpty()
                                        where o.StatusPayment != (int)EPaymentOutOfStockStatus.Paied && (loggedModel.RoleId != (int)ERoleId.Sale || acc.SaleId == loggedModel.Id)
                                        select o
                                    ).CountAsync();

            var newNotifications = await _unitOfWork.Repository<Notification>().GetQueryable().Where(x =>
                        x.AccountId == loggedModel.Id
                         //&& x.IsRead == false 
                         && x.IsStaff == true
                    ).OrderByDescending(x => x.Id).Take(20).ToListAsync();
            return new AccountHomeResponse
            {
                Id = loggedModel.Id,
                Avatar = loggedModel.Avatar,
                Fullname = loggedModel.FullName,
                RoleName = ERoleIdName.GetRoleName(loggedModel.RoleId),
                Username = loggedModel.Username,
                CountOrder = countOrderList,
                NewNotifications = newNotifications,
                UnPayOutOfStock = unPayOutOfStock
            };
        }

        public async Task<ChartResponse> WeekStatistic(int? id, DateTime dataDate)
        {
            var response = new ChartResponse();
            DateTime from, to;

            // Xác định ngày bắt đầu tuần (thứ Hai) và ngày kết thúc tuần (Chủ Nhật)
            if (dataDate.DayOfWeek == DayOfWeek.Sunday)
            {
                from = dataDate.AddDays(-6).Date; // Lùi về thứ Hai trước đó
                to = dataDate.Date; // Chủ Nhật của tuần hiện tại
            }
            else
            {
                from = dataDate.AddDays(DayOfWeek.Monday - dataDate.DayOfWeek).Date;
                to = from.AddDays(6); // Cộng thêm 6 ngày để đến Chủ Nhật
            }

            // Lọc các đơn hàng có DateCompleted nằm trong khoảng từ thứ Hai đến Chủ Nhật
            var query = _unitOfWork.Repository<Transportation>()
                .GetQueryable()
                .Where(x => x.DateCompleted.HasValue &&
                            x.DateCompleted.Value.Date >= from &&
                            x.DateCompleted.Value.Date <= to &&
                            x.Status == (int)ETransportationStatus.Completed &&
                            (id == null || x.StaffId == id)
                            );

            response.Order = await query.CountAsync();
            response.Weight = await query.SumAsync(x => x.Weight ?? 0);
            response.Volume = await query.SumAsync(x => x.Volume ?? 0);
            response.TotalPriceVND = await query.SumAsync(x => x.TotalPriceVND);

            return response;
        }

        public async Task<double[]> WeekStatisticData(int? id, DateTime dataDate, int type)
        {
            var response = new double[7]; // Mảng kết quả cho 7 ngày trong tuần

            DateTime from, to;

            if (dataDate.DayOfWeek == DayOfWeek.Sunday)
            {
                from = dataDate.AddDays(-6).Date;
            }
            else
            {
                from = dataDate.AddDays(DayOfWeek.Monday - dataDate.DayOfWeek).Date;
            }

            to = from.AddDays(6);

            var query = await _unitOfWork.Repository<Transportation>()
                .GetQueryable()
                .Where(x => x.DateCompleted.HasValue &&
                            x.DateCompleted.Value.Date >= from &&
                            x.DateCompleted.Value.Date <= to &&
                            x.Status == (int)ETransportationStatus.Completed &&
                            (id == null || x.StaffId == id)
                            )
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                DateTime currentDay = from.AddDays(i);
                var dailyData = query.Where(x => x.DateCompleted.Value.Date == currentDay);

                switch (type)
                {
                    case 0: // Số lượng đơn hàng
                        response[i] = dailyData.Count();
                        break;
                    case 1: // Tổng trọng lượng Weight
                        response[i] = dailyData.Sum(x => x.Weight ?? 0);
                        break;
                    case 2: // Tổng thể tích Volume
                        response[i] = dailyData.Sum(x => x.Volume ?? 0);
                        break;
                    case 3: // Tổng tiền TotalPriceVND
                        response[i] = (double)dailyData.Sum(x => x.TotalPriceVND);
                        break;
                    default:
                        response[i] = 0;
                        break;
                }
            }

            return response;
        }

        public async Task<ChartResponse> MonthStatistic(int? id, DateTime dataDate)
        {
            var response = new ChartResponse();

            // Xác định ngày đầu tiên và ngày cuối cùng của tháng
            DateTime from = new DateTime(dataDate.Year, dataDate.Month, 1); // Ngày đầu tháng
            DateTime to = from.AddMonths(1).AddDays(-1); // Ngày cuối tháng

            // Lọc các đơn hàng có DateCompleted nằm trong tháng
            var query = _unitOfWork.Repository<Transportation>()
                .GetQueryable()
                .Where(x => x.DateCompleted.HasValue &&
                            x.DateCompleted.Value.Date >= from &&
                            x.DateCompleted.Value.Date <= to &&
                            x.Status == (int)ETransportationStatus.Completed &&
                            (id == null || x.StaffId == id)
                            );

            response.Order = await query.CountAsync();
            response.Weight = await query.SumAsync(x => x.Weight ?? 0);
            response.Volume = await query.SumAsync(x => x.Volume ?? 0);
            response.TotalPriceVND = await query.SumAsync(x => x.TotalPriceVND);

            return response;
        }

        public async Task<double[]> MonthStatisticData(int? id, DateTime dataDate, int type)
        {
            // Xác định ngày đầu và cuối tháng
            DateTime from = new DateTime(dataDate.Year, dataDate.Month, 1);
            DateTime to = from.AddMonths(1).AddDays(-1);

            int totalDays = (to - from).Days + 1; // Số ngày trong tháng
            var response = new double[totalDays]; // Mảng kết quả theo từng ngày

            // Lấy tất cả dữ liệu trong tháng một lần
            var query = await _unitOfWork.Repository<Transportation>()
                .GetQueryable()
                .Where(x => x.DateCompleted.HasValue &&
                            x.DateCompleted.Value.Date >= from &&
                            x.DateCompleted.Value.Date <= to &&
                            x.Status == (int)ETransportationStatus.Completed &&
                            (id == null || x.StaffId == id)
                            )
                .ToListAsync();

            for (int i = 0; i < totalDays; i++)
            {
                DateTime currentDay = from.AddDays(i);
                var dailyData = query.Where(x => x.DateCompleted.Value.Date == currentDay);

                switch (type)
                {
                    case 0: // Số lượng đơn hàng
                        response[i] = dailyData.Count();
                        break;
                    case 1: // Tổng trọng lượng Weight
                        response[i] = dailyData.Sum(x => x.Weight ?? 0);
                        break;
                    case 2: // Tổng thể tích Volume
                        response[i] = dailyData.Sum(x => x.Volume ?? 0);
                        break;
                    case 3: // Tổng tiền TotalPriceVND
                        response[i] = (double)dailyData.Sum(x => x.TotalPriceVND);
                        break;
                    default:
                        response[i] = 0;
                        break;
                }
            }

            return response;
        }

        public async Task<ChartResponse> YearStatistic(int? id, DateTime dataDate)
        {
            var response = new ChartResponse();

            // Xác định ngày đầu và cuối năm
            DateTime from = new DateTime(dataDate.Year, 1, 1); // Ngày đầu năm
            DateTime to = new DateTime(dataDate.Year, 12, 31); // Ngày cuối năm

            // Lọc các đơn hàng có DateCompleted nằm trong tháng
            var query = _unitOfWork.Repository<Transportation>()
                .GetQueryable()
                .Where(x => x.DateCompleted.HasValue &&
                            x.DateCompleted.Value.Date >= from &&
                            x.DateCompleted.Value.Date <= to &&
                            x.Status == (int)ETransportationStatus.Completed &&
                            (id == null || x.StaffId == id)
                            );

            response.Order = await query.CountAsync();
            response.Weight = await query.SumAsync(x => x.Weight ?? 0);
            response.Volume = await query.SumAsync(x => x.Volume ?? 0);
            response.TotalPriceVND = await query.SumAsync(x => x.TotalPriceVND);

            return response;
        }

        public async Task<double[]> YearStatisticData(int? id, DateTime dataDate, int type)
        {
            // Xác định ngày đầu và cuối năm
            DateTime from = new DateTime(dataDate.Year, 1, 1); // Ngày đầu năm
            DateTime to = new DateTime(dataDate.Year, 12, 31); // Ngày cuối năm

            var response = new double[12]; // Mảng kết quả cho 12 tháng

            // Lấy tất cả dữ liệu trong năm một lần
            var query = await _unitOfWork.Repository<Transportation>()
                .GetQueryable()
                .Where(x => x.DateCompleted.HasValue &&
                            x.DateCompleted.Value.Date >= from &&
                            x.DateCompleted.Value.Date <= to &&
                            x.Status == (int)ETransportationStatus.Completed &&
                            (id == null || x.StaffId == id)
                            )
                .ToListAsync();

            // Lặp qua 12 tháng
            for (int month = 0; month < 12; month++)
            {
                DateTime firstDayOfMonth = new DateTime(dataDate.Year, month + 1, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1); // Ngày cuối cùng của tháng

                var monthlyData = query.Where(x => x.DateCompleted.Value.Date >= firstDayOfMonth &&
                                                   x.DateCompleted.Value.Date <= lastDayOfMonth);

                switch (type)
                {
                    case 0: // Số lượng đơn hàng
                        response[month] = monthlyData.Count();
                        break;
                    case 1: // Tổng trọng lượng Weight
                        response[month] = monthlyData.Sum(x => x.Weight ?? 0);
                        break;
                    case 2: // Tổng thể tích Volume
                        response[month] = monthlyData.Sum(x => x.Volume ?? 0);
                        break;
                    case 3: // Tổng tiền TotalPriceVND
                        response[month] = (double)monthlyData.Sum(x => x.TotalPriceVND);
                        break;
                    default:
                        response[month] = 0;
                        break;
                }
            }
            return response;
        }

        public async Task<List<StatisticTable12Response>> StatisticTable12(int? id, DateTime? fromDate, DateTime? toDate, int? shipId = null)
        {
            var response = new List<StatisticTable12Response>();
            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (toDate == null)
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
            var currentAccount = _httpContextService.GetLoggedModel();
            if (currentAccount.RoleId == (int)ERoleId.Sale)
            {
                id = currentAccount.Id;
            }
            var query = from t in _unitOfWork.Repository<Transportation>().GetQueryable()
                        join a in _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.RoleId == (int)ERoleId.User && (id == null || x.SaleId == id))
                        on t.AccountId equals a.Id
                        where t.DateCompleted.HasValue &&
                              t.DateCompleted.Value.Date >= fromDate &&
                              t.DateCompleted.Value.Date <= toDate &&
                            (id == null || t.StaffId == id) &&
                            (shipId == null || t.ShipId == shipId)
                        group new { t, a } by new { t.AccountId, a.Username, a.Phone } into g
                        select new StatisticTable12Response
                        {
                            Username = g.Key.Username,
                            Phone = g.Key.Phone,
                            Order = g.Count(),
                            TotalPriceVND = g.Sum(x => x.t.TotalPriceVND)
                        };
            response = await query.ToListAsync();
            return response;
        }

        public async Task<List<StatisticTable3Response>> StatisticTable3(int? id, DateTime? fromDate, DateTime? toDate, bool? isOrder)
        {
            var response = new List<StatisticTable3Response>();
            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (toDate == null)
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
            var query = StatisticTable3Query(id, fromDate, toDate);
            response = await query.Where(x => (isOrder == null || isOrder == x.IsOrder)).OrderByDescending(x => x.Id).ToListAsync();
            return response;
        }
        public async Task<StatisticTable3TitleResponse> StatisticTitleTable3(int? id, DateTime? fromDate, DateTime? toDate)
        {
            var response = new StatisticTable3TitleResponse();
            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (toDate == null)
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
            var query = StatisticTable3Query(id, fromDate, toDate);
            response.Title1 = await query.CountAsync();
            response.Title2 = await query.Where(x => x.IsOrder == true).CountAsync();
            return response;
        }

        private IQueryable<StatisticTable3Response> StatisticTable3Query(int? id, DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (toDate == null)
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);

            var query = from a in _unitOfWork.Repository<Account>().GetQueryable()
                        .Where(x => x.RoleId == (int)ERoleId.User && x.Created >= fromDate && x.Created <= toDate &&
                            (id == null || x.SaleId == id)
                            )
                        join t in _unitOfWork.Repository<Transportation>().GetQueryable()
                        on a.Id equals t.AccountId into trans
                        from t in trans.DefaultIfEmpty()
                        group new { a, t } by new { a.Id, a.Username, a.Phone, a.Created } into g
                        select new StatisticTable3Response
                        {
                            Id = g.Key.Id,
                            Username = g.Key.Username,
                            Phone = g.Key.Phone,
                            Created = g.Key.Created.ToString("dd/MM/yyyy"),
                            IsOrder = g.Any(x => x.t != null),
                        };
            return query;
        }

        public async Task<List<StatisticTable4Response>> StatisticTable4(int? id, DateTime? fromDate, DateTime? toDate)
        {
            var response = new List<StatisticTable4Response>();

            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (toDate == null)
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);

            var query = from t in _unitOfWork.Repository<Transportation>().GetQueryable()
                        join a in _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.RoleId == (int)ERoleId.User &&
                            (id == null || x.SaleId == id)
                            )
                        on t.AccountId equals a.Id
                        where t.DateCompleted >= fromDate && t.DateCompleted <= toDate
                        group t by new { a.Id, a.Username, a.Phone } into g
                        where g.All(t => t.DateCompleted >= fromDate)
                        select new StatisticTable4Response
                        {
                            Id = g.Key.Id,
                            Username = g.Key.Username,
                            Phone = g.Key.Phone,
                            TotalPriceVND = g.Sum(t => t.TotalPriceVND)
                        };
            response = await query.OrderByDescending(x => x.TotalPriceVND).ToListAsync();

            return response;
        }


        public async Task<List<StatisticTabl56Response>> StatisticTable5(int? id)
        {
            var response = new List<StatisticTabl56Response>();

            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var query =
                from a in _unitOfWork.Repository<Account>().GetQueryable()
                .Where(x => x.RoleId == (int)ERoleId.User &&
                            (id == null || x.SaleId == id)
                            ) // Lọc tài khoản có RoleId là User
                join t in _unitOfWork.Repository<Transportation>().GetQueryable()
                    .Where(t => t.DateCompleted != null) // Lọc các đơn có ngày hoàn thành
                    on a.Id equals t.AccountId into gj
                from t in gj.DefaultIfEmpty() // LEFT JOIN để lấy cả tài khoản chưa có đơn hàng
                group t by new { a.Id, a.Username, a.Phone } into g
                select new
                {
                    g.Key.Id,
                    g.Key.Username,
                    g.Key.Phone,
                    LastOrder = g.Max(t => t.DateCompleted) // Tính ngày đặt đơn gần nhất
                };

            // Chuyển dữ liệu đã tính toán từ cơ sở dữ liệu sang dạng DTO
            response = await query
                .Where(x => x.LastOrder == null || x.LastOrder < sixMonthsAgo)
                .Select(x => new StatisticTabl56Response
                {
                    Id = x.Id,
                    Username = x.Username,
                    Phone = x.Phone,
                    LastOrder = x.LastOrder.Value.ToString("dd/MM/yyyy") ?? "",
                    DifferenceDay = x.LastOrder.HasValue ? (DateTime.Now - x.LastOrder.Value).Days : -1
                })
                .ToListAsync();

            return response;
        }

        public async Task<List<StatisticTabl56Response>> StatisticTable6(int? id)
        {
            var response = new List<StatisticTabl56Response>();

            var threeMonthsAgo = DateTime.Now.AddMonths(-3);

            var query =
                from a in _unitOfWork.Repository<Account>().GetQueryable()
                .Where(x => x.RoleId == (int)ERoleId.User &&
                            (id == null || x.SaleId == id)
                            ) // Lọc tài khoản có RoleId là User
                join t in _unitOfWork.Repository<Transportation>().GetQueryable()
                    .Where(t => t.DateCompleted != null
                            ) // Lọc các đơn có ngày hoàn thành
                    on a.Id equals t.AccountId into gj
                from t in gj.DefaultIfEmpty() // LEFT JOIN để lấy cả tài khoản chưa có đơn hàng
                group t by new { a.Id, a.Username, a.Phone } into g
                select new
                {
                    g.Key.Id,
                    g.Key.Username,
                    g.Key.Phone,
                    LastOrder = g.Max(t => t.DateCompleted) // Tính ngày đặt đơn gần nhất
                };

            // Chuyển dữ liệu đã tính toán từ cơ sở dữ liệu sang dạng DTO
            response = await query
                .Where(x => x.LastOrder == null || x.LastOrder < threeMonthsAgo)
                .Select(x => new StatisticTabl56Response
                {
                    Id = x.Id,
                    Username = x.Username,
                    Phone = x.Phone,
                    LastOrder = x.LastOrder.Value.ToString("dd/MM/yyyy") ?? "",
                    DifferenceDay = x.LastOrder.HasValue ? (DateTime.Now - x.LastOrder.Value).Days : -1
                })
                .ToListAsync();

            return response;
        }

        public async Task<ColumnChartResponse> ReportFeeYearChartData(DateTime dataDate)
        {
            var response = new ColumnChartResponse();
            response.Col1 = new decimal[12];
            response.Col2 = new decimal[12];
            response.Col3 = new decimal[12];
            response.Col4 = new decimal[12];
            for (int month = 0; month < 12; month++)
            {
                DateTime firstDayOfMonth = new DateTime(dataDate.Year, month + 1, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                response.Col1[month] = await _unitOfWork.Repository<Transportation>()
                                        .GetQueryable()
                                        .Where(x => x.DateCompleted.HasValue &&
                                                    x.Status == (int)ETransportationStatus.Completed &&
                                                    x.DateCompleted.Value.Date >= firstDayOfMonth &&
                                                    x.DateCompleted.Value.Date <= lastDayOfMonth)
                                        .SumAsync(x => x.TotalPriceVND);
                response.Col2[month] = await _unitOfWork.Repository<ReportFixedFee>()
                                        .GetQueryable()
                                        .Where(x => x.Status == 2 &&
                                                    x.DataDate.Date >= firstDayOfMonth &&
                                                    x.DataDate.Date <= lastDayOfMonth)
                                        .SumAsync(x => x.Amount);
                response.Col3[month] = await _unitOfWork.Repository<ReportOtherFee>()
                                        .GetQueryable()
                                        .Where(x => x.Status == 2 &&
                                                    x.DataDate.Date >= firstDayOfMonth &&
                                                    x.DataDate.Date <= lastDayOfMonth)
                                        .SumAsync(x => x.Amount);
                response.Col4[month] = await _unitOfWork.Repository<ReportPartnerFee>()
                                        .GetQueryable()
                                        .Where(x => x.DataDate.Date >= firstDayOfMonth &&
                                                    x.DataDate.Date <= lastDayOfMonth)
                                        .SumAsync(x => x.Amount);
            }
            return response;
        }

        public async Task<ColumnChartResponse> ReportFeePostOfficeChartData(DateTime dataDate)
        {
            var response = new ColumnChartResponse();
            var postOffices = PostOfficeName.GetPostOffice();
            int totalColumn = postOffices.Count;
            response.Col1 = new decimal[totalColumn];
            response.Col2 = new decimal[totalColumn];
            response.Col3 = new decimal[totalColumn];
            response.Col4 = new decimal[totalColumn];

            DateTime firstDayOfMonth = new DateTime(dataDate.Year, dataDate.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var revenueQuery = _unitOfWork.Repository<Transportation>()
                                        .GetQueryable()
                                        .Where(x => x.DateCompleted.HasValue
                                                    && x.Status == (int)ETransportationStatus.Completed &&
                                                    x.DateCompleted.Value.Date >= firstDayOfMonth &&
                                                    x.DateCompleted.Value.Date <= lastDayOfMonth);
            var fixedFeeQuery = _unitOfWork.Repository<ReportFixedFee>()
                                        .GetQueryable()
                                        .Where(x => x.Status == 2 &&
                                                    x.DataDate.Date >= firstDayOfMonth &&
                                                    x.DataDate.Date <= lastDayOfMonth);
            var otherFeeQuery = _unitOfWork.Repository<ReportOtherFee>()
                                        .GetQueryable()
                                        .Where(x => x.Status == 2 &&
                                                    x.DataDate.Date >= firstDayOfMonth &&
                                                    x.DataDate.Date <= lastDayOfMonth);
            var partnerFeeQuery = _unitOfWork.Repository<ReportPartnerFee>()
                                        .GetQueryable()
                                        .Where(x => x.DataDate.Date >= firstDayOfMonth &&
                                                    x.DataDate.Date <= lastDayOfMonth);
            for (int i = 0; i < totalColumn; i++)
            {
                response.Col1[i] = await revenueQuery.Where(x => x.PostOffice == postOffices[i]).SumAsync(x => x.TotalPriceVND);
                response.Col2[i] = await fixedFeeQuery.Where(x => x.PostOffice == (i + 1)).SumAsync(x => x.Amount);
                response.Col3[i] = await otherFeeQuery.Where(x => x.PostOffice == (i + 1)).SumAsync(x => x.Amount);
                response.Col4[i] = await partnerFeeQuery.Where(x => x.PostOffice == (i + 1)).SumAsync(x => x.Amount);
            }

            return response;
        }

        public async Task<ColumnChartResponse> ReportNewAccountChartData(DateTime dataDate)
        {
            var response = new ColumnChartResponse();
            response.Col1 = new decimal[12];
            response.Col2 = new decimal[12];
            response.Col3 = new decimal[12];
            response.Col4 = new decimal[12];
            var postOffices = PostOfficeName.GetPostOffice();

            for (int month = 0; month < 12; month++)
            {
                DateTime firstDayOfMonth = new DateTime(dataDate.Year, month + 1, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var newAccountQuery = _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.RoleId == (int)ERoleId.User && x.Created.Date >= firstDayOfMonth && x.Created <= lastDayOfMonth);
                response.Col1[month] = await newAccountQuery.Where(x => x.PostOffice == postOffices[0]).CountAsync();
                response.Col2[month] = await newAccountQuery.Where(x => x.PostOffice == postOffices[1]).CountAsync();
                response.Col3[month] = await newAccountQuery.Where(x => x.PostOffice == postOffices[2]).CountAsync();
                response.Col4[month] = await newAccountQuery.Where(x => x.PostOffice == postOffices[3]).CountAsync();
            }
            return response;
        }
        public async Task<ColumnChartResponse> ReportAccountOrderedChartData(DateTime dataDate)
        {
            var response = new ColumnChartResponse();
            response.Col1 = new decimal[12];
            response.Col2 = new decimal[12];
            response.Col3 = new decimal[12];
            response.Col4 = new decimal[12];
            var postOffices = PostOfficeName.GetPostOffice();

            for (int month = 0; month < 12; month++)
            {
                DateTime firstDayOfMonth = new DateTime(dataDate.Year, month + 1, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var accountIdInTransporations = await _unitOfWork.Repository<Transportation>()
                                                    .GetQueryable()
                                                    .Where(x => x.DateCompleted.HasValue &&
                                                        x.Status == (int)ETransportationStatus.Completed &&
                                                        x.DateCompleted.Value.Date >= firstDayOfMonth &&
                                                        x.DateCompleted.Value.Date <= lastDayOfMonth
                                                    ).Select(x => x.AccountId).Distinct().ToListAsync();

                var orderdAccountQuery = _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.RoleId == (int)ERoleId.User && accountIdInTransporations.Contains(x.Id));
                response.Col1[month] = await orderdAccountQuery.Where(x => x.PostOffice == postOffices[0]).CountAsync();
                response.Col2[month] = await orderdAccountQuery.Where(x => x.PostOffice == postOffices[1]).CountAsync();
                response.Col3[month] = await orderdAccountQuery.Where(x => x.PostOffice == postOffices[2]).CountAsync();
                response.Col4[month] = await orderdAccountQuery.Where(x => x.PostOffice == postOffices[3]).CountAsync();
            }
            return response;
        }

        public async Task<PagedList<ReportRevenueResponse>> GetReportRevenuePaging(ReportRevenueSearch search)
        {
            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            DateTime fromDate = new DateTime(dataDate.Year, dataDate.Month, 1);
            DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

            var query = GetReportRevenueQuery(fromDate, toDate, search.PostOffice, search.AccountId, search.SaleId);
            int total = await query.CountAsync();
            var datas = await query.OrderByDescending(x => x.TotalPriceVND)
                .Skip((search.PageIndex - 1) * search.PageSize).Take(search.PageSize)
                .ToListAsync();
            decimal totalPriceVNDAll = await query.SumAsync(x => x.TotalPriceVND);
            if (datas.Any())
            {
                datas[0].TableTitle = $"Doanh Số: {dataDate.Month}/{dataDate.Year}: Số Tiền:&nbsp; <strong>{string.Format("{0:N0}", totalPriceVNDAll)}</strong> &nbsp;VND";
            }
            return new PagedList<ReportRevenueResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = datas
            };
        }

        public async Task<ChartResponse> ReportRevenueYearStatistic(ReportRevenueSearch search)
        {
            var response = new ChartResponse();

            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            // Xác định ngày đầu và cuối năm
            DateTime fromDate = new DateTime(dataDate.Year, 1, 1); // Ngày đầu năm
            DateTime toDate = new DateTime(dataDate.Year, 12, 31); // Ngày cuối năm

            // Lọc các đơn hàng có DateCompleted nằm trong tháng
            var query = GetReportRevenueQuery(fromDate, toDate, search.PostOffice, search.AccountId, search.SaleId);

            response.Order = await query.CountAsync();
            response.Weight = await query.SumAsync(x => x.Weight);
            response.Volume = await query.SumAsync(x => x.Volume);
            response.TotalPriceVND = await query.SumAsync(x => x.TotalPriceVND);

            return response;
        }

        public async Task<double[]> ReportRevenueYearStatisticData(ReportRevenueSearch search)
        {
            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            // Xác định ngày đầu và cuối năm
            DateTime fromDate = new DateTime(dataDate.Year, 1, 1); // Ngày đầu năm
            DateTime toDate = new DateTime(dataDate.Year, 12, 31); // Ngày cuối năm

            var response = new double[12]; // Mảng kết quả cho 12 tháng

            // Lặp qua 12 tháng
            for (int month = 0; month < 12; month++)
            {
                DateTime firstDayOfMonth = new DateTime(dataDate.Year, month + 1, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1); // Ngày cuối cùng của tháng

                switch (search.Type)
                {
                    case 0: // Số lượng đơn hàng
                        response[month] = await GetReportRevenueQuery(firstDayOfMonth, lastDayOfMonth, search.PostOffice, search.AccountId, search.SaleId).CountAsync();
                        break;
                    case 1: // Tổng trọng lượng Weight
                        response[month] = await GetReportRevenueQuery(firstDayOfMonth, lastDayOfMonth, search.PostOffice, search.AccountId, search.SaleId).SumAsync(x => x.Weight);
                        break;
                    case 2: // Tổng thể tích Volume
                        response[month] = await GetReportRevenueQuery(firstDayOfMonth, lastDayOfMonth, search.PostOffice, search.AccountId, search.SaleId).SumAsync(x => x.Volume);
                        break;
                    case 3: // Tổng tiền TotalPriceVND
                        response[month] = (double)await GetReportRevenueQuery(firstDayOfMonth, lastDayOfMonth, search.PostOffice, search.AccountId, search.SaleId).SumAsync(x => x.TotalPriceVND);
                        break;
                    default:
                        response[month] = 0;
                        break;
                }
            }
            return response;
        }


        public async Task<PagedList<ReportFixedFeeResponse>> GetReportFixedFeePaging(ReportFeeSearch search)
        {
            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            DateTime fromDate = new DateTime(dataDate.Year, dataDate.Month, 1);
            DateTime toDate = fromDate.AddMonths(1).AddDays(-1);
            var query = GetReportFixedFeeQuery(fromDate, toDate, search.Type);
            var fees = await query.OrderByDescending(x => x.Id)
                .Skip((search.PageIndex - 1) * search.PageSize).Take(search.PageSize)
                .ToListAsync();
            int total = await query.CountAsync();
            decimal totalPriceVNDAll = await query.Where(x => x.Status == 2).SumAsync(x => x.Amount);
            if (fees.Any())
            {
                fees[0].TableTitle = $"Tổng Chi Phí Tháng: {dataDate.Month}/{dataDate.Year}: Số Tiền:&nbsp; <strong>{string.Format("{0:N0}", totalPriceVNDAll)}</strong> &nbsp;VND";
            }
            return new PagedList<ReportFixedFeeResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = fees
            };
        }

        public async Task<PagedList<ReportOtherFeeResponse>> GetReportOtherFeePaging(ReportFeeSearch search)
        {
            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            DateTime fromDate = new DateTime(dataDate.Year, dataDate.Month, 1);
            DateTime toDate = fromDate.AddMonths(1).AddDays(-1);
            var query = GetReportOtherFeeQuery(fromDate, toDate, search.Type);
            var fees = await query.OrderByDescending(x => x.Id)
                .Skip((search.PageIndex - 1) * search.PageSize).Take(search.PageSize)
                .ToListAsync();
            int total = await query.CountAsync();
            decimal totalPriceVNDAll = await query.Where(x => x.Status == 2).SumAsync(x => x.Amount);
            if (fees.Any())
            {
                fees[0].TableTitle = $"Tổng Chi Phí Tháng: {dataDate.Month}/{dataDate.Year}: Số Tiền:&nbsp; <strong>{string.Format("{0:N0}", totalPriceVNDAll)}</strong> &nbsp;VND";
            }
            return new PagedList<ReportOtherFeeResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = fees
            };
        }

        public async Task<PagedList<ReportPartnerFeeResponse>> GetReportPartnerFeePaging(ReportFeeSearch search)
        {
            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            DateTime fromDate = new DateTime(dataDate.Year, dataDate.Month, 1);
            DateTime toDate = fromDate.AddMonths(1).AddDays(-1);
            var query = GetReportPartnerFeeQuery(fromDate, toDate, search.Type);
            var fees = await query.OrderByDescending(x => x.Id)
                .Skip((search.PageIndex - 1) * search.PageSize).Take(search.PageSize)
                .ToListAsync();
            int total = await query.CountAsync();
            decimal totalPriceVNDAll = await query.SumAsync(x => x.Amount);
            if (fees.Any())
            {
                fees[0].TableTitle = $"Tổng Chi Phí Tháng: {dataDate.Month}/{dataDate.Year}: Số Tiền:&nbsp; <strong>{string.Format("{0:N0}", totalPriceVNDAll)}</strong> &nbsp;VND";
            }
            return new PagedList<ReportPartnerFeeResponse>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = fees
            };
        }

        public async Task<decimal[]> ReportFixedFeeYearStatisticData(ReportFeeSearch search)
        {
            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            var response = new decimal[12]; // Mảng kết quả cho 12 tháng
            // Lặp qua 12 tháng
            for (int month = 0; month < 12; month++)
            {
                DateTime firstDayOfMonth = new DateTime(dataDate.Year, month + 1, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1); // Ngày cuối cùng của tháng

                response[month] = await GetReportFixedFeeQuery(firstDayOfMonth, lastDayOfMonth, search.Type).Where(x => x.Status == 2).SumAsync(x => x.Amount);
            }
            return response;
        }

        public async Task<decimal[]> ReportOtherFeeYearStatisticData(ReportFeeSearch search)
        {
            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            var response = new decimal[12]; // Mảng kết quả cho 12 tháng
            // Lặp qua 12 tháng
            for (int month = 0; month < 12; month++)
            {
                DateTime firstDayOfMonth = new DateTime(dataDate.Year, month + 1, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1); // Ngày cuối cùng của tháng

                response[month] = await GetReportOtherFeeQuery(firstDayOfMonth, lastDayOfMonth, search.Type).Where(x => x.Status == 2).SumAsync(x => x.Amount);
            }
            return response;
        }

        public async Task<ChartResponse> ReportPartnerFeeYearStatistic(ReportFeeSearch search)
        {
            var response = new ChartResponse();
            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            // Xác định ngày đầu và cuối năm
            DateTime fromDate = new DateTime(dataDate.Year, 1, 1); // Ngày đầu năm
            DateTime toDate = new DateTime(dataDate.Year, 12, 31); // Ngày cuối năm

            // Lọc các đơn hàng có DateCompleted nằm trong tháng
            var query = GetReportPartnerFeeQuery(fromDate, toDate, search.Type);

            response.Weight = await query.SumAsync(x => x.Weight);
            response.Volume = await query.SumAsync(x => x.Volume);
            response.TotalPriceVND = await query.SumAsync(x => x.Amount);

            return response;
        }

        public async Task<double[]> ReportPartnerFeeYearStatisticData(ReportFeeSearch search)
        {
            var dataDate = DateTime.Now;
            if (search.FromDate != null)
            {
                dataDate = search.FromDate.Value;
            }
            var response = new double[12]; // Mảng kết quả cho 12 tháng

            // Lặp qua 12 tháng
            for (int month = 0; month < 12; month++)
            {
                DateTime firstDayOfMonth = new DateTime(dataDate.Year, month + 1, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1); // Ngày cuối cùng của tháng

                switch (search.DataType)
                {
                    case 0: // Số lượng đơn hàng
                        response[month] = await GetReportPartnerFeeQuery(firstDayOfMonth, lastDayOfMonth, search.Type).SumAsync(x => x.Weight);
                        break;
                    case 1: // Tổng trọng lượng Weight
                        response[month] = await GetReportPartnerFeeQuery(firstDayOfMonth, lastDayOfMonth, search.Type).SumAsync(x => x.Volume);
                        break;
                    case 2: // Tổng thể tích Volume
                        response[month] = (double)await GetReportPartnerFeeQuery(firstDayOfMonth, lastDayOfMonth, search.Type).SumAsync(x => x.Amount);
                        break;

                    default:
                        response[month] = 0;
                        break;
                }
            }
            return response;
        }

        private IQueryable<ReportRevenueResponse> GetReportRevenueQuery(DateTime fromDate, DateTime toDate, int? postOffice, int? accountId, int? saleId)
        {
            string postOfficeName = null;
            if (postOffice != null)
            {
                postOfficeName = PostOfficeName.GetPostOffice().ElementAt((postOffice - 1) ?? 0);
            }
            var query = from t in _unitOfWork.Repository<Transportation>().GetQueryable()
                        join a in _unitOfWork.Repository<Account>().GetQueryable().Where(x => x.RoleId == (int)ERoleId.User)
                        on t.AccountId equals a.Id
                        where t.DateCompleted.HasValue &&
                              t.DateCompleted.Value.Date >= fromDate &&
                              t.DateCompleted.Value.Date <= toDate &&
                              (postOfficeName == null || t.PostOffice == postOfficeName) &&
                              (accountId == null || t.AccountId == accountId) &&
                              (saleId == null || t.StaffId == saleId)
                        group new { t, a } by new { t.AccountId, a.Username, a.Phone } into g
                        select new ReportRevenueResponse
                        {
                            Username = g.Key.Username,
                            AccountId = g.Key.AccountId ?? 0,
                            Order = g.Count(),
                            Weight = g.Sum(x => x.t.Weight ?? 0),
                            Volume = g.Sum(x => x.t.Volume ?? 0),
                            TotalPriceVND = g.Sum(x => x.t.TotalPriceVND)
                        };
            return query;
        }

        private IQueryable<ReportFixedFeeResponse> GetReportFixedFeeQuery(DateTime fromDate, DateTime toDate, int? postOffice)
        {
            var query = from fee in _unitOfWork.Repository<ReportFixedFee>().GetQueryable()
                        join account in _unitOfWork.Repository<Account>().GetQueryable()
                        on fee.CreatedBy equals account.Id
                        where fee.DataDate.Date >= fromDate && fee.DataDate.Date <= toDate
                            && (postOffice == null || fee.PostOffice == postOffice)
                        select new ReportFixedFeeResponse
                        {
                            Id = fee.Id,
                            Status = fee.Status,
                            DataDate = fee.DataDate,
                            Amount = fee.Amount,
                            DetailFile = fee.DetailFile,
                            Name = fee.Name,
                            PostOffice = fee.PostOffice,
                            Username = account.Username,
                        };
            return query;
        }

        private IQueryable<ReportOtherFeeResponse> GetReportOtherFeeQuery(DateTime fromDate, DateTime toDate, int? postOffice)
        {
            var query = from fee in _unitOfWork.Repository<ReportOtherFee>().GetQueryable()
                        join account in _unitOfWork.Repository<Account>().GetQueryable()
                        on fee.CreatedBy equals account.Id
                        where fee.DataDate.Date >= fromDate && fee.DataDate.Date <= toDate
                            && (postOffice == null || fee.PostOffice == postOffice)
                        select new ReportOtherFeeResponse
                        {
                            Id = fee.Id,
                            Status = fee.Status,
                            DataDate = fee.DataDate,
                            Amount = fee.Amount,
                            DetailFile = fee.DetailFile,
                            Name = fee.Name,
                            PostOffice = fee.PostOffice,
                            Username = account.Username,
                        };
            return query;
        }

        private IQueryable<ReportPartnerFeeResponse> GetReportPartnerFeeQuery(DateTime fromDate, DateTime toDate, int? postOffice)
        {
            var query = from fee in _unitOfWork.Repository<ReportPartnerFee>().GetQueryable()
                        join account in _unitOfWork.Repository<Account>().GetQueryable()
                        on fee.CreatedBy equals account.Id
                        where fee.DataDate.Date >= fromDate && fee.DataDate.Date <= toDate
                            && (postOffice == null || fee.PostOffice == postOffice)
                        select new ReportPartnerFeeResponse
                        {
                            Id = fee.Id,
                            DataDate = fee.DataDate,
                            Amount = fee.Amount,
                            Code = fee.Code,
                            Note = fee.Note,
                            Volume = fee.Volume,
                            Weight = fee.Weight,
                            PostOffice = fee.PostOffice,
                            Username = account.Username,
                        };
            return query;
        }

        public async Task<bool> CreateReportFixedFee(CreateFixedFeeRequest request)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var fileDetail = await _uploadFileService.UploadFile(request.DetailFile);
            var reportFixedFee = new ReportFixedFee
            {
                Name = request.Name,
                PostOffice = request.PostOffice,
                Amount = request.Amount,
                DataDate = request.DataDate,
                DetailFile = fileDetail,
            };
            await _unitOfWork.Repository<ReportFixedFee>().Add(reportFixedFee, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> CreateReportOtherFee(CreateOtherFeeRequest request)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var fileDetail = await _uploadFileService.UploadFile(request.DetailFile);
            var reportOtherFee = new ReportOtherFee
            {
                Name = request.Name,
                PostOffice = request.PostOffice,
                Amount = request.Amount,
                DataDate = request.DataDate,
                DetailFile = fileDetail,
            };
            await _unitOfWork.Repository<ReportOtherFee>().Add(reportOtherFee, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> CreateReportPartnerFee(CreatePartnerFeeRequest request)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var reportPartnerFee = new ReportPartnerFee
            {
                Code = request.Code,
                PostOffice = request.PostOffice,
                Amount = request.Amount,
                DataDate = request.DataDate,
                Note = request.Note,
                Volume = request.Volume,
                Weight = request.Weight,
            };
            await _unitOfWork.Repository<ReportPartnerFee>().Add(reportPartnerFee, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> UpdateReportFixedFee(int id, CreateFixedFeeRequest request)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var fileDetail = await _uploadFileService.UploadFile(request.DetailFile);
            var reportFixedFee = await _unitOfWork.Repository<ReportFixedFee>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id && x.Status == 1);
            reportFixedFee.Name = request.Name;
            reportFixedFee.Amount = request.Amount;
            reportFixedFee.DataDate = request.DataDate;
            if (fileDetail?.Length > 0)
            {
                reportFixedFee.DetailFile = fileDetail;
            }
            _unitOfWork.Repository<ReportFixedFee>().Update(reportFixedFee, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> UpdateReportOtherFee(int id, CreateOtherFeeRequest request)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var fileDetail = await _uploadFileService.UploadFile(request.DetailFile);
            var reportOtherFee = await _unitOfWork.Repository<ReportOtherFee>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id && x.Status == 1);
            reportOtherFee.Name = request.Name;
            reportOtherFee.Amount = request.Amount;
            reportOtherFee.DataDate = request.DataDate;
            if (fileDetail?.Length > 0)
            {
                reportOtherFee.DetailFile = fileDetail;
            }
            _unitOfWork.Repository<ReportOtherFee>().Update(reportOtherFee, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> UpdateReportPartnerFee(int id, CreatePartnerFeeRequest request)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var reportPartnerFee = await _unitOfWork.Repository<ReportPartnerFee>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            reportPartnerFee.Code = request.Code;
            reportPartnerFee.Amount = request.Amount;
            reportPartnerFee.DataDate = request.DataDate;
            reportPartnerFee.Note = request.Note;
            reportPartnerFee.Volume = request.Volume;
            reportPartnerFee.Weight = request.Weight;
            _unitOfWork.Repository<ReportPartnerFee>().Update(reportPartnerFee, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> AcceptReportFixedFee(int id)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var reportFixedFee = await _unitOfWork.Repository<ReportFixedFee>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id && x.Status == 1);
            reportFixedFee.Status = 2;
            _unitOfWork.Repository<ReportFixedFee>().Update(reportFixedFee, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> AcceptReportOtherFee(int id)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var reportOtherFee = await _unitOfWork.Repository<ReportOtherFee>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id && x.Status == 1);
            reportOtherFee.Status = 2;
            _unitOfWork.Repository<ReportOtherFee>().Update(reportOtherFee, DateTime.Now, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }
        public async Task<bool> DeleteReportPartnerFee(int id)
        {
            var currentAccount = _httpContextService.GetLoggedModel();
            var reportOtherFee = await _unitOfWork.Repository<ReportPartnerFee>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
            _unitOfWork.Repository<ReportPartnerFee>().Delete(reportOtherFee);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<ChartResponse> YearStatisticOfAccount(int id, DateTime dataDate)
        {
            var response = new ChartResponse();

            // Xác định ngày đầu và cuối năm
            DateTime from = new DateTime(dataDate.Year, 1, 1); // Ngày đầu năm
            DateTime to = new DateTime(dataDate.Year, 12, 31); // Ngày cuối năm

            // Lọc các đơn hàng có DateCompleted nằm trong tháng
            var query = _unitOfWork.Repository<Transportation>()
                .GetQueryable()
                .Where(x => x.DateCompleted.HasValue &&
                            x.Status == (int)ETransportationStatus.Completed &&
                            x.DateCompleted.Value.Date >= from &&
                            x.DateCompleted.Value.Date <= to &&
                            x.AccountId == id
                            );

            response.Order = await query.CountAsync();
            response.Weight = await query.SumAsync(x => x.Weight ?? 0);
            response.Volume = await query.SumAsync(x => x.Volume ?? 0);
            response.TotalPriceVND = await query.SumAsync(x => x.TotalPriceVND);

            return response;
        }

        public async Task<YearStatisticDataOfAccountResponse> YearStatisticDataOfAccount(int id, DateTime dataDate, int type)
        {
            // Xác định ngày đầu năm
            DateTime from = new DateTime(dataDate.Year, 1, 1); // Ngày đầu năm

            var query = _unitOfWork.Repository<Transportation>()
                .GetQueryable()
                .Where(x => x.DateCompleted.HasValue &&
                            x.Status == (int)ETransportationStatus.Completed &&
                            x.AccountId == id
                            );

            var datas = new List<double>();
            var titles = new List<string>();
            for (int i = 0; i < dataDate.DayOfYear; i++)
            {
                DateTime firstDay = from.AddDays(i);
                DateTime nextDay = firstDay.AddDays(1);

                var dailyData = query.Where(x => x.DateCompleted.Value.Date >= firstDay &&
                                                   x.DateCompleted.Value.Date < nextDay);
                switch (type)
                {
                    case 0: // Số lượng đơn hàng
                        var data0 = await dailyData.CountAsync();
                        if (data0 > 0)
                        {
                            datas.Add(data0);
                            titles.Add($"{firstDay.Day.ToString("00")}.{firstDay.Month.ToString("00")}");
                        }
                        break;
                    case 1: // Tổng trọng lượng Weight
                        var data1 = dailyData.Sum(x => x.Weight ?? 0);
                        if (data1 > 0)
                        {
                            datas.Add(data1);
                            titles.Add($"{firstDay.Day.ToString("00")}.{firstDay.Month.ToString("00")}");
                        }
                        break;
                    case 2: // Tổng thể tích Volume
                        var data2 = dailyData.Sum(x => x.Volume ?? 0);
                        if (data2 > 0)
                        {
                            datas.Add(data2);
                            titles.Add($"{firstDay.Day.ToString("00")}.{firstDay.Month.ToString("00")}");
                        }
                        break;
                    case 3: // Tổng tiền TotalPriceVND
                        var data3 = (double)dailyData.Sum(x => x.TotalPriceVND);
                        if (data3 > 0)
                        {
                            datas.Add(data3);
                            titles.Add($"{firstDay.Day.ToString("00")}.{firstDay.Month.ToString("00")}");
                        }
                        break;
                    default:
                        datas.Add(0);
                        break;
                }
            }
            return new YearStatisticDataOfAccountResponse
            {
                Data = datas.ToArray(),
                Title = titles.ToArray(),
            };
        }

    }
}

