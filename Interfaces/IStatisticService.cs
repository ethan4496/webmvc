using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;

namespace WebMVC.Interfaces
{
    public interface IStatisticService
    {
        Task<bool> AcceptReportFixedFee(int id);
        Task<bool> AcceptReportOtherFee(int id);
        Task<bool> CreateReportFixedFee(CreateFixedFeeRequest request);
        Task<bool> CreateReportOtherFee(CreateOtherFeeRequest request);
        Task<bool> CreateReportPartnerFee(CreatePartnerFeeRequest request);
        Task<bool> DeleteReportPartnerFee(int id);
        Task<AccountHomeResponse> GetDashboard();
        Task<PagedList<ReportFixedFeeResponse>> GetReportFixedFeePaging(ReportFeeSearch search);
        Task<PagedList<ReportOtherFeeResponse>> GetReportOtherFeePaging(ReportFeeSearch search);
        Task<PagedList<ReportPartnerFeeResponse>> GetReportPartnerFeePaging(ReportFeeSearch search);
        Task<PagedList<ReportRevenueResponse>> GetReportRevenuePaging(ReportRevenueSearch search);
        Task<ChartResponse> MonthStatistic(int? id, DateTime dataDate);
        Task<double[]> MonthStatisticData(int? id, DateTime dataDate, int type);
        Task<ColumnChartResponse> ReportAccountOrderedChartData(DateTime dataDate);
        Task<ColumnChartResponse> ReportFeePostOfficeChartData(DateTime dataDate);
        Task<ColumnChartResponse> ReportFeeYearChartData(DateTime dataDate);
        Task<decimal[]> ReportFixedFeeYearStatisticData(ReportFeeSearch search);
        Task<ColumnChartResponse> ReportNewAccountChartData(DateTime dataDate);
        Task<decimal[]> ReportOtherFeeYearStatisticData(ReportFeeSearch search);
        Task<ChartResponse> ReportPartnerFeeYearStatistic(ReportFeeSearch search);
        Task<double[]> ReportPartnerFeeYearStatisticData(ReportFeeSearch search);
        Task<ChartResponse> ReportRevenueYearStatistic(ReportRevenueSearch search);
        Task<double[]> ReportRevenueYearStatisticData(ReportRevenueSearch search);
        Task<List<StatisticTable12Response>> StatisticTable12(int? id, DateTime? fromDate, DateTime? toDate, int? shipId = null);
        Task<List<StatisticTable3Response>> StatisticTable3(int? id, DateTime? fromDate, DateTime? toDate, bool? isOrder);
        Task<List<StatisticTable4Response>> StatisticTable4(int? id, DateTime? fromDate, DateTime? toDate);
        Task<List<StatisticTabl56Response>> StatisticTable5(int? id);
        Task<List<StatisticTabl56Response>> StatisticTable6(int? id);
        Task<StatisticTable3TitleResponse> StatisticTitleTable3(int? id, DateTime? fromDate, DateTime? toDate);
        Task<bool> UpdateReportFixedFee(int id, CreateFixedFeeRequest request);
        Task<bool> UpdateReportOtherFee(int id, CreateOtherFeeRequest request);
        Task<bool> UpdateReportPartnerFee(int id, CreatePartnerFeeRequest request);
        Task<ChartResponse> WeekStatistic(int? id, DateTime dataDate);
        Task<double[]> WeekStatisticData(int? id, DateTime dataDate, int type);
        Task<ChartResponse> YearStatistic(int? id, DateTime dataDate);
        Task<double[]> YearStatisticData(int? id, DateTime dataDate, int type);
        Task<YearStatisticDataOfAccountResponse> YearStatisticDataOfAccount(int id, DateTime dataDate, int type);
        Task<ChartResponse> YearStatisticOfAccount(int id, DateTime dataDate);
    }
}
