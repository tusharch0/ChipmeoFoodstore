using FoodstoreApi.Infrastructure.Handlers;
using FoodstoreApi.Infrastructure.Repositories;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodstoreApi.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ISourceRepository, SourceRepository>();
        services.AddScoped<IAddonRepository, AddonRepository>();
        services.AddScoped<IDiscountRepository, DiscountRepository>();
        services.AddScoped<IComboRepository, ComboRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IPaymentSettingRepository, PaymentSettingRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentIntentRepository, PaymentIntentRepository>();
        services.AddScoped<IPaymentEventRepository, PaymentEventRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
        services.AddScoped<IBlogRevisionRepository, BlogRevisionRepository>();
        services.AddScoped<IBlogBlockRepository, BlogBlockRepository>();
        services.AddScoped<IBlogSettingRepository, BlogSettingRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();

        services.AddScoped<IEInvoiceRepository, EInvoiceRepository>();

        services.AddScoped<IMediaService, MediaHandler>();

        return services;
    }
}
