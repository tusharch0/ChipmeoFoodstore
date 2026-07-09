using FoodstoreApi.Usecase.DTOs.Report;

namespace FoodstoreApi.Usecase.Interfaces;

public interface IReportService
{
    Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<DashboardStatsDto> GetStatsAsync(DateTime fromDate, DateTime toDate, string groupBy, CancellationToken cancellationToken = default);
    Task<SalesForecastDto> GetForecastAsync(int horizon = 7, CancellationToken cancellationToken = default);
    Task<DashboardLegacyDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<List<ComboRecommendationDto>> GetComboRecommendationsAsync(CancellationToken cancellationToken = default);
    Task<FinancialReportDto> GetFinancialReportAsync(FinancialReportRequest request, CancellationToken cancellationToken = default);
}




