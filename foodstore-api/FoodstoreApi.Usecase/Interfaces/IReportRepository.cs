using FoodstoreApi.Core.Entities;
using FoodstoreApi.Usecase.DTOs.Report;

namespace FoodstoreApi.Usecase.Interfaces;

public interface IReportRepository
{
    Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<DashboardStatsDto> GetStatsAsync(DateTime fromDate, DateTime toDate, string groupBy, CancellationToken cancellationToken = default);
    Task<List<SalesDataDto>> GetSalesDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<DashboardLegacyDto> GetLegacyDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<List<ComboRecommendationDataDto>> GetComboRecommendationDataAsync(DateTime fromDate, CancellationToken cancellationToken = default);
    Task<FinancialReportDto> GetFinancialReportAsync(FinancialReportRequest request, CancellationToken cancellationToken = default);
}

public class SalesDataDto
{
    public DateTime Date { get; set; }
    public float Revenue { get; set; }
}

public class ComboRecommendationDataDto
{
    public Guid? MenuItemId { get; set; }
    public string? MenuItemName { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid OrderId { get; set; }
}





