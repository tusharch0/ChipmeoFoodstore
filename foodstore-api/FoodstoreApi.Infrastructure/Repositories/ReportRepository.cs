using FoodstoreApi.Core.Constants;
using FoodstoreApi.Core.Entities;
using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.DTOs.Report;
using FoodstoreApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Infrastructure.Repositories;

public class ReportRepository(StoreDbContext context) : IReportRepository
{
    private readonly StoreDbContext _context = context;

    public async Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayOrdersQuery = _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow && OrderStatus.ActiveSet.Contains(o.Status!));

        var todayRevenue = await todayOrdersQuery.SumAsync(o => o.TotalAmount ?? 0, cancellationToken);
        var todayVat = await todayOrdersQuery.SumAsync(o => o.VatAmount ?? 0, cancellationToken);
        var todayOrdersCount = await todayOrdersQuery.CountAsync(cancellationToken);

        var avgOrderValue = todayOrdersCount > 0 ? todayRevenue / todayOrdersCount : 0;

        var hourlyOrders = await todayOrdersQuery
            .GroupBy(o => o.CreatedAt.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync(cancellationToken);

        var peakHour = hourlyOrders != null ? $"{hourlyOrders.Hour}:00 - {hourlyOrders.Hour + 1}:00" : "N/A";

        var popularItemsList = await _context.OrderItems.AsNoTracking()
            .Where(i => i.Order.CreatedAt >= today && i.Order.CreatedAt < tomorrow && OrderStatus.ActiveSet.Contains(i.Order.Status!))
            .GroupBy(i => i.MenuItemName)
            .Select(g => new { Name = g.Key, Sold = g.Sum(i => i.Quantity) })
            .OrderByDescending(x => x.Sold)
            .Take(5)
            .ToListAsync(cancellationToken);

        var popularItems = popularItemsList.Select(x => new PopularItemDto(x.Name, x.Sold)).ToList();

        var serviceStats = await todayOrdersQuery
            .GroupBy(o => o.Source != null ? o.Source.Name : "Khác")
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var sourceStats = serviceStats.Select(x => new PopularItemDto(x.Name, x.Count)).ToList();

        return new DashboardOverviewDto(
            todayRevenue,
            todayVat,
            todayOrdersCount,
            avgOrderValue,
            peakHour,
            popularItems,
            sourceStats
        );
    }

    public async Task<DashboardStatsDto> GetStatsAsync(DateTime fromDate, DateTime toDate, string groupBy, CancellationToken cancellationToken = default)
    {
        var ordersQuery = _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate && OrderStatus.ActiveSet.Contains(o.Status!));

        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount ?? 0, cancellationToken);
        var totalVat = await ordersQuery.SumAsync(o => o.VatAmount ?? 0, cancellationToken);
        var totalOrders = await ordersQuery.CountAsync(cancellationToken);

        List<ChartDataDto> revenueChart;
        List<ChartDataDto> ordersChart;

        var ordersList = await ordersQuery
            .Select(o => new { o.CreatedAt, o.TotalAmount })
            .ToListAsync(cancellationToken);

        if (groupBy == "month")
        {
            var grouped = ordersList
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(x => x.TotalAmount ?? 0),
                    Orders = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToList();

            revenueChart = grouped.Select(x => new ChartDataDto(x.Label, x.Revenue)).ToList();
            ordersChart = grouped.Select(x => new ChartDataDto(x.Label, x.Orders)).ToList();
        }
        else
        {
            var grouped = ordersList
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new
                {
                    Label = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(x => x.TotalAmount ?? 0),
                    Orders = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToList();

            revenueChart = grouped.Select(x => new ChartDataDto(x.Label, x.Revenue)).ToList();
            ordersChart = grouped.Select(x => new ChartDataDto(x.Label, x.Orders)).ToList();
        }

        var topItemsList = await _context.OrderItems.AsNoTracking()
            .Where(i => i.Order.CreatedAt >= fromDate && i.Order.CreatedAt <= toDate && OrderStatus.ActiveSet.Contains(i.Order.Status!) && i.MenuItemId != null)
            .GroupBy(i => i.MenuItemName)
            .Select(g => new { Name = g.Key, Sold = g.Sum(i => i.Quantity) })
            .OrderByDescending(x => x.Sold)
            .Take(10)
            .ToListAsync(cancellationToken);

        var topItems = topItemsList.Select(x => new PopularItemDto(x.Name, x.Sold)).ToList();

        var rawComboItems = await _context.OrderItems.AsNoTracking()
            .Where(i => i.Order.CreatedAt >= fromDate && i.Order.CreatedAt <= toDate && OrderStatus.ActiveSet.Contains(i.Order.Status!) && (i.ComboId != null || i.MenuItemId == null))
            .Include(i => i.Combo)
            .Select(i => new 
            { 
                i.ComboId,
                ComboName = i.Combo != null ? i.Combo.Name : null,
                i.MenuItemName,
                i.Quantity
            })
            .ToListAsync(cancellationToken);

        var popularCombos = rawComboItems
            .Select(i => 
            {
                if (!string.IsNullOrEmpty(i.ComboName)) return new { Name = i.ComboName, i.Quantity };
                if (!string.IsNullOrEmpty(i.MenuItemName)) return new { Name = i.MenuItemName, i.Quantity };
                return new { Name = "Unknown Combo", i.Quantity };
            })
            .GroupBy(x => x.Name)
            .Select(g => new PopularItemDto(g.Key, g.Sum(x => x.Quantity)))
            .OrderByDescending(x => x.Sold)
            .Take(10)
            .ToList();

        var serviceStats = await ordersQuery
            .GroupBy(o => o.Source != null ? o.Source.Name : "Khác")
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var sourceStats = serviceStats.Select(x => new PopularItemDto(x.Name, x.Count)).ToList();

        return new DashboardStatsDto(
            totalRevenue,
            totalVat,
            totalOrders,
            revenueChart,
            ordersChart,
            topItems,
            popularCombos,
            sourceStats
        );
    }

    public async Task<List<SalesDataDto>> GetSalesDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt < toDate && OrderStatus.ActiveSet.Contains(o.Status!))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesDataDto
            {
                Date = g.Key,
                Revenue = (float)g.Sum(o => o.TotalAmount ?? 0)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<DashboardLegacyDto> GetLegacyDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var paidOrders = await _context.Orders.AsNoTracking()
            .Where(o => OrderStatus.ActiveSet.Contains(o.Status!))
            .Include(o => o.OrderItems)
            .Include(o => o.Source)
            .ToListAsync(cancellationToken);

        var todayOrders = paidOrders.Where(o => o.CreatedAt >= todayStart).ToList();
        var todayRevenue = todayOrders.Sum(o => o.TotalAmount ?? 0);

        var monthOrders = paidOrders.Where(o => o.CreatedAt >= monthStart).ToList();
        var monthRevenue = monthOrders.Sum(o => o.TotalAmount ?? 0);

        var totalRevenue = paidOrders.Sum(o => o.TotalAmount ?? 0);

        var popularItemsList = await _context.OrderItems.AsNoTracking()
            .Where(oi => OrderStatus.ActiveSet.Contains(oi.Order.Status!) && oi.Order.CreatedAt >= todayStart && oi.Order.CreatedAt < todayStart.AddDays(1) && oi.MenuItemId != null)
            .GroupBy(oi => new { oi.MenuItemId, Name = oi.MenuItemName })
            .Select(g => new
            {
                Name = g.Key.Name,
                Quantity = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToListAsync(cancellationToken);

        var popularItems = popularItemsList.Select(x => new PopularItemLegacyDto(x.Name, x.Quantity, x.Revenue)).ToList();

        var popularCombosList = await _context.OrderItems.AsNoTracking()
            .Where(oi => OrderStatus.ActiveSet.Contains(oi.Order.Status!) && oi.Order.CreatedAt >= todayStart && oi.Order.CreatedAt < todayStart.AddDays(1) && oi.ComboId != null)
            .GroupBy(oi => new { oi.ComboId, Name = oi.MenuItemName })
            .Select(g => new
            {
                Name = g.Key.Name,
                Quantity = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToListAsync(cancellationToken);

        var popularCombos = popularCombosList.Select(x => new PopularItemLegacyDto(x.Name, x.Quantity, x.Revenue)).ToList();

        var last7Days = Enumerable.Range(0, 7).Select(i => todayStart.AddDays(-i)).ToList();
        var dailyRevenue = last7Days.Select(day =>
        {
            var dayOrders = paidOrders.Where(o => o.CreatedAt.Date == day).ToList();
            return new DailyRevenueDto(
                day.ToString("yyyy-MM-dd"),
                dayOrders.Sum(o => o.TotalAmount ?? 0),
                dayOrders.Count
            );
        }).OrderBy(x => x.Date).ToList();

        var last6Months = Enumerable.Range(0, 6).Select(i =>
        {
            var month = monthStart.AddMonths(-i);
            return new DateTime(month.Year, month.Month, 1);
        }).ToList();

        var monthlyRevenue = last6Months.Select(month =>
        {
            var nextMonth = month.AddMonths(1);
            var mOrders = paidOrders.Where(o => o.CreatedAt >= month && o.CreatedAt < nextMonth).ToList();
            return new MonthlyRevenueDto(
                month.ToString("yyyy-MM"),
                mOrders.Sum(o => o.TotalAmount ?? 0),
                mOrders.Count
            );
        }).OrderBy(x => x.Month).ToList();

        var avgOrderValue = paidOrders.Any() ? paidOrders.Average(o => o.TotalAmount ?? 0) : 0;

        var hourlyOrders = paidOrders
            .GroupBy(o => o.CreatedAt.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefault();

        var peakHour = hourlyOrders != null ? $"{hourlyOrders.Hour}:00 - {hourlyOrders.Hour + 1}:00" : "N/A";

        var payments = await _context.Payments.AsNoTracking()
            .Where(p => p.Status == "success")
            .GroupBy(p => p.Method)
            .Select(g => new
            {
                Method = g.Key,
                Amount = g.Sum(p => p.Amount)
            })
            .ToListAsync(cancellationToken);

        var paymentBreakdown = payments.ToDictionary(p => p.Method, p => p.Amount);

        var totalCustomers = paidOrders.Select(o => o.SourceId).Distinct().Count();

        var serviceStats = todayOrders
            .GroupBy(o => o.Source != null ? o.Source.Name : "Khác")
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        var sourceStats = serviceStats.Select(x => new PopularItemLegacyDto(x.Name, x.Count, 0)).ToList();

        return new DashboardLegacyDto(
            new { Revenue = todayRevenue, Orders = todayOrders.Count },
            new { Revenue = monthRevenue, Orders = monthOrders.Count },
            new { Revenue = totalRevenue, Orders = paidOrders.Count },
            popularItems,
            popularCombos,
            dailyRevenue,
            monthlyRevenue,
            avgOrderValue,
            peakHour,
            paymentBreakdown,
            totalCustomers,
            sourceStats
        );
    }

    public async Task<List<ComboRecommendationDataDto>> GetComboRecommendationDataAsync(DateTime fromDate, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CreatedAt >= fromDate && OrderStatus.ActiveSet.Contains(o.Status!))
            .SelectMany(o => o.OrderItems
                .Where(oi => oi.MenuItemId != null)
                .Select(oi => new ComboRecommendationDataDto
                {
                    MenuItemId = oi.MenuItemId,
                    MenuItemName = oi.MenuItemName,
                    UnitPrice = oi.UnitPrice,
                    OrderId = o.Id
                }))
            .ToListAsync(cancellationToken);
    }

    public async Task<FinancialReportDto> GetFinancialReportAsync(FinancialReportRequest request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken)
            ?? throw new InvalidOperationException("Organization was not found.");
        if (request.BranchId.HasValue && !await _context.Branches.IgnoreQueryFilters().AnyAsync(
                b => b.Id == request.BranchId.Value && b.OrganizationId == request.OrganizationId, cancellationToken))
            throw new InvalidOperationException("Branch does not belong to the organization.");

        var zone = ResolveTimeZone(organization.TimeZone);
        var from = TimeZoneInfo.ConvertTimeToUtc(request.FromDate.ToDateTime(TimeOnly.MinValue), zone);
        var to = TimeZoneInfo.ConvertTimeToUtc(request.ToDate.AddDays(1).ToDateTime(TimeOnly.MinValue), zone);
        var branches = await _context.Branches.IgnoreQueryFilters().AsNoTracking()
            .Where(b => b.OrganizationId == request.OrganizationId && (!request.BranchId.HasValue || b.Id == request.BranchId.Value))
            .OrderBy(b => b.Name).Select(b => new { b.Id, b.Name }).ToListAsync(cancellationToken);
        var branchIds = branches.Select(b => b.Id).ToList();

        var orders = await _context.Orders.IgnoreQueryFilters().AsNoTracking()
            .Where(o => o.BranchId.HasValue && branchIds.Contains(o.BranchId.Value) && o.CreatedAt >= from && o.CreatedAt < to)
            .GroupBy(o => o.BranchId!.Value).Select(g => new { BranchId = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        var paymentAttempts = await _context.PaymentIntents.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.BranchId.HasValue && branchIds.Contains(p.BranchId.Value) && p.UpdatedAt >= from && p.UpdatedAt < to)
            .GroupBy(p => new { p.BranchId, p.Status }).Select(g => new { g.Key.BranchId, g.Key.Status, Count = g.Count(), Amount = g.Sum(x => x.Amount) }).ToListAsync(cancellationToken);
        var journals = await _context.LedgerJournals.IgnoreQueryFilters().AsNoTracking().Include(j => j.Lines).ThenInclude(l => l.Account)
            .Where(j => j.OrganizationId == request.OrganizationId && j.PostedAt >= from && j.PostedAt < to && (!request.BranchId.HasValue || j.BranchId == request.BranchId))
            .ToListAsync(cancellationToken);
        var walletLines = await _context.LedgerJournalLines.IgnoreQueryFilters().AsNoTracking()
            .Where(l => l.Account.OrganizationId == request.OrganizationId && l.Account.Code == "RESTAURANT_WALLET" && l.Account.Currency == organization.CurrencyCode)
            .Select(l => new { l.Account.BranchId, Amount = l.Credit - l.Debit }).ToListAsync(cancellationToken);
        var settlements = await _context.SettlementBatches.IgnoreQueryFilters().AsNoTracking().Include(batch => batch.Lines)
            .Where(b => b.OrganizationId == request.OrganizationId && b.PeriodDate >= request.FromDate && b.PeriodDate <= request.ToDate)
            .ToListAsync(cancellationToken);
        var settlementIntentIds = settlements.SelectMany(batch => batch.Lines).Select(line => line.PaymentIntentId).Distinct().ToList();
        var settlementBranches = await _context.PaymentIntents.IgnoreQueryFilters().AsNoTracking()
            .Where(intent => settlementIntentIds.Contains(intent.Id) && intent.BranchId.HasValue)
            .ToDictionaryAsync(intent => intent.Id, intent => intent.BranchId!.Value, cancellationToken);
        bool SettlementContainsSelectedBranch(SettlementBatch batch) => !request.BranchId.HasValue ||
            batch.Lines.Any(line => settlementBranches.GetValueOrDefault(line.PaymentIntentId) == request.BranchId.Value);
        decimal SettlementAmount(SettlementBatch batch) => !request.BranchId.HasValue ? batch.NetAmount :
            batch.Lines.Where(line => settlementBranches.GetValueOrDefault(line.PaymentIntentId) == request.BranchId.Value).Sum(line => line.NetAmount);

        var summaries = branches.Select(branch =>
        {
            var orderCount = orders.Where(x => x.BranchId == branch.Id).Sum(x => x.Count);
            var paymentJournals = journals.Where(j => j.BranchId == branch.Id && j.EntryType == "payment_confirmed").ToList();
            var refundJournals = journals.Where(j => j.BranchId == branch.Id && j.EntryType == "payment_refunded").ToList();
            var gross = paymentJournals.SelectMany(j => j.Lines).Where(l => l.Account.Code == "PROVIDER_CLEARING").Sum(l => l.Debit);
            var refunds = refundJournals.SelectMany(j => j.Lines).Where(l => l.Account.Code == "RESTAURANT_WALLET").Sum(l => l.Debit);
            var net = paymentJournals.Concat(refundJournals).SelectMany(j => j.Lines)
                .Where(l => l.Account.Code == "RESTAURANT_WALLET").Sum(l => l.Credit - l.Debit);
            var branchWallet = walletLines.Where(line => line.BranchId == branch.Id).Sum(line => line.Amount);
            return new BranchFinancialSummary(branch.Id, branch.Name, orderCount, paymentJournals.Count,
                orderCount == 0 ? 0 : Math.Round(paymentJournals.Count * 100m / orderCount, 2), gross, refunds, net, branchWallet);
        }).ToList();
        var commissions = journals.SelectMany(j => j.Lines).Where(l => l.Account.Code == "PLATFORM_COMMISSION").Sum(l => l.Credit);
        var providerFees = journals.SelectMany(j => j.Lines).Where(l => l.Account.Code == "PROVIDER_FEES").Sum(l => l.Debit);
        var failedPayments = paymentAttempts.Where(x => x.Status == "failed").Sum(x => x.Count);
        var refundsAwaiting = paymentAttempts.Where(x => x.Status == "refund_pending").Sum(x => x.Count);
        var reconciliationCount = await _context.ReconciliationCases.IgnoreQueryFilters().AsNoTracking()
            .CountAsync(c => c.OrganizationId == request.OrganizationId && c.Status != "resolved" && (!request.BranchId.HasValue || c.BranchId == request.BranchId), cancellationToken);
        var walletPosition = walletLines.Where(line => !request.BranchId.HasValue || line.BranchId == request.BranchId).Sum(line => line.Amount);
        var totals = new FinancialReportTotals(summaries.Sum(s => s.Orders), summaries.Sum(s => s.SuccessfulPayments),
            summaries.Sum(s => s.Orders) == 0 ? 0 : Math.Round(summaries.Sum(s => s.SuccessfulPayments) * 100m / summaries.Sum(s => s.Orders), 2),
            summaries.Sum(s => s.GrossSales), summaries.Sum(s => s.Refunds), commissions, providerFees,
            summaries.Sum(s => s.NetSales), walletPosition,
            settlements.Where(s => s.Status is not "paid" and not "reconciled" && SettlementContainsSelectedBranch(s)).Sum(SettlementAmount),
            settlements.Count(SettlementContainsSelectedBranch));
        var exceptions = new FinancialExceptionSummary(failedPayments, refundsAwaiting,
            walletPosition < 0 || settlements.Any(s => s.Holds > 0 && SettlementContainsSelectedBranch(s)) ? 1 : 0,
            settlements.Count(s => s.Status == "failed" && SettlementContainsSelectedBranch(s)), reconciliationCount);

        _context.ReportAccessLogs.Add(new ReportAccessLog { Id = Guid.NewGuid(), OrganizationId = request.OrganizationId, BranchId = request.BranchId,
            ActorId = request.ActorId, ReportType = "financial", FromDate = request.FromDate, ToDate = request.ToDate, IsExport = request.IsExport });
        await _context.SaveChangesAsync(cancellationToken);
        return new FinancialReportDto(request.OrganizationId, organization.CurrencyCode, organization.TimeZone, request.FromDate, request.ToDate, totals, summaries, exceptions);
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZone)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(timeZone); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time"); }
    }
}




