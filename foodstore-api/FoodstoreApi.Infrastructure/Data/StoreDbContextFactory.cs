using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FoodstoreApi.Infrastructure.Data;

/// <summary>
/// Keeps EF tooling independent from the web host, its logging providers, and live runtime services.
/// </summary>
public sealed class StoreDbContextFactory : IDesignTimeDbContextFactory<StoreDbContext>
{
    public StoreDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseNpgsql("Host=localhost;Database=foodstore_design;Username=postgres;Password=postgres")
            .Options;

        return new StoreDbContext(options);
    }
}
