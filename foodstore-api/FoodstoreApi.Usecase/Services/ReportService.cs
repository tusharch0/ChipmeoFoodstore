using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.DTOs.Report;
using FoodstoreApi.Usecase.Utils;
using FoodstoreApi.Core.Constants;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace FoodstoreApi.Usecase.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ITenantContext _tenantContext;

    public ReportService(IReportRepository repository, IDistributedCache cache, ITenantContext tenantContext)
    {
        _repository = repository;
        _cache = cache;
        _tenantContext = tenantContext;
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(CacheKeys.Dashboard.Overview(_tenantContext.CacheScope), () =>
            _repository.GetOverviewAsync(cancellationToken), TimeSpan.FromMinutes(3), cancellationToken);
    }

    public async Task<DashboardStatsDto> GetStatsAsync(DateTime fromDate, DateTime toDate, string groupBy, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Dashboard.Stats(_tenantContext.CacheScope, fromDate.ToString("yyyyMMdd"), toDate.ToString("yyyyMMdd"), groupBy);
        return await _cache.GetOrSetAsync(key, () =>
            _repository.GetStatsAsync(fromDate, toDate, groupBy, cancellationToken), TimeSpan.FromMinutes(3), cancellationToken);
    }

    public async Task<SalesForecastDto> GetForecastAsync(int horizon = 7, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(CacheKeys.Dashboard.Forecast(_tenantContext.CacheScope, horizon), async () =>
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-90);

            var salesData = await _repository.GetSalesDataAsync(startDate, endDate, cancellationToken);

            var fullData = new List<SalesData>();
            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var existing = salesData.FirstOrDefault(x => x.Date == date);
                fullData.Add(new SalesData { Date = date, Revenue = existing?.Revenue ?? 0 });
            }

            if (fullData.Count < 7)
            {
                return new SalesForecastDto(new List<ForecastDataDto>());
            }

            var mlContext = new MLContext();
            var dataView = mlContext.Data.LoadFromEnumerable(fullData);

            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedRevenue",
                inputColumnName: nameof(SalesData.Revenue),
                windowSize: 7,
                seriesLength: fullData.Count,
                trainSize: fullData.Count,
                horizon: horizon,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBoundRevenue",
                confidenceUpperBoundColumn: "UpperBoundRevenue");

            var model = forecastingPipeline.Fit(dataView);
            var forecastingEngine = model.CreateTimeSeriesEngine<SalesData, SalesPrediction>(mlContext);
            var prediction = forecastingEngine.Predict();

            var forecasts = new List<ForecastDataDto>();
            for (int i = 0; i < horizon; i++)
            {
                var date = endDate.AddDays(i);
                var revenue = prediction.ForecastedRevenue[i];
                forecasts.Add(new ForecastDataDto(date.ToString("yyyy-MM-dd"), Math.Max(0, revenue)));
            }

            string recommendation = "Dữ liệu chưa đủ để phân tích xu hướng chiến lược.";
            try
            {
                var stats = await GetStatsAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "day", cancellationToken);
                var typeStats = stats.SourceStats;

                if (typeStats != null && typeStats.Count != 0)
                {
                    var total = typeStats.Sum(x => x.Sold);
                    if (total > 0)
                    {
                        var topType = typeStats.OrderByDescending(x => x.Sold).First();
                        var percent = (double)topType.Sold / total * 100;

                        var isOffline = topType.Name.Contains("quán", StringComparison.OrdinalIgnoreCase) ||
                                       topType.Name.Contains("Source", StringComparison.OrdinalIgnoreCase) ||
                                       topType.Name.Contains("Offline", StringComparison.OrdinalIgnoreCase);

                        if (isOffline)
                        {
                            recommendation = $"💡 AI Insight: Khách hàng chủ yếu dùng bữa **tại quán** ({percent:F0}%). \n👉 Chiến lược: Tạo không gian trải nghiệm check-in, upsell trực tiếp combo đồ uống và món tráng miệng.";
                        }
                        else
                        {
                            recommendation = $"💡 AI Insight: Xu hướng đặt món **{topType.Name}** đang chiếm ưu thế ({percent:F0}%). \n👉 Chiến lược: Đẩy mạnh quảng cáo trên nền tảng Online, thiết kế bao bì bắt mắt và tối ưu thời gian giao hàng.";
                        }
                    }
                }
            }
            catch
            {
            }

            return new SalesForecastDto(forecasts, recommendation);
        }, TimeSpan.FromMinutes(5), cancellationToken);
    }

    public class SalesData
    {
        public DateTime Date { get; set; }
        public float Revenue { get; set; }
    }

    public class SalesPrediction
    {
        public float[] ForecastedRevenue { get; set; } = null!;
        public float[] LowerBoundRevenue { get; set; } = null!;
        public float[] UpperBoundRevenue { get; set; } = null!;
    }

    public async Task<DashboardLegacyDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetLegacyDashboardStatsAsync(cancellationToken);
    }

    public async Task<List<ComboRecommendationDto>> GetComboRecommendationsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(CacheKeys.Dashboard.ComboRecommendations(_tenantContext.CacheScope), async () =>
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var rawData = await _repository.GetComboRecommendationDataAsync(cutoffDate, cancellationToken);

            var pairCounts = new Dictionary<(Guid, Guid), int>();
            var itemInfo = new Dictionary<Guid, (string Name, decimal Price)>();

            var orders = rawData.GroupBy(x => x.OrderId);

            foreach (var order in orders)
            {
                var uniqueItems = order.DistinctBy(i => i.MenuItemId).ToList();

                for (int i = 0; i < uniqueItems.Count; i++)
                {
                    if (!itemInfo.ContainsKey(uniqueItems[i].MenuItemId!.Value))
                    {
                        itemInfo[uniqueItems[i].MenuItemId!.Value] = (uniqueItems[i].MenuItemName!, uniqueItems[i].UnitPrice);
                    }

                    for (int j = i + 1; j < uniqueItems.Count; j++)
                    {
                        var id1 = uniqueItems[i].MenuItemId!.Value;
                        var id2 = uniqueItems[j].MenuItemId!.Value;

                        var key = string.Compare(id1.ToString(), id2.ToString(), StringComparison.Ordinal) < 0 ? (id1, id2) : (id2, id1);

                        if (!pairCounts.ContainsKey(key))
                            pairCounts[key] = 0;

                        pairCounts[key]++;
                    }
                }
            }

            var topPairs = pairCounts.OrderByDescending(kvp => kvp.Value).Take(5);

            var recommendations = new List<ComboRecommendationDto>();
            foreach (var pair in topPairs)
            {
                var item1 = itemInfo[pair.Key.Item1];
                var item2 = itemInfo[pair.Key.Item2];
                var totalOriginal = item1.Price + item2.Price;

                var discounted = totalOriginal * 0.9m;
                var suggested = Math.Round(discounted / 1000m) * 1000m;

                recommendations.Add(new ComboRecommendationDto
                {
                    Item1Name = item1.Name,
                    Item2Name = item2.Name,
                    Frequency = pair.Value,
                    TotalOriginalPrice = totalOriginal,
                    SuggestedPrice = suggested,
                    Reason = $"Hai món này được gọi cùng nhau {pair.Value} lần trong 30 ngày qua."
                });
            }

            return recommendations;
        }, TimeSpan.FromMinutes(10), cancellationToken);
    }

    public Task<FinancialReportDto> GetFinancialReportAsync(FinancialReportRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ToDate < request.FromDate)
            throw new InvalidOperationException("The report end date must not be earlier than the start date.");
        if (request.ToDate.DayNumber - request.FromDate.DayNumber > 366)
            throw new InvalidOperationException("Financial reports support a maximum period of 366 days.");
        return _repository.GetFinancialReportAsync(request, cancellationToken);
    }
}
