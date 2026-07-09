using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FoodstoreApi.Usecase.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAddonService, AddonService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IComboService, ComboService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISourceService, SourceService>();
        services.AddScoped<IPaymentSettingService, PaymentSettingService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IBlogCategoryService, BlogCategoryService>();
        services.AddScoped<IBlogRevisionService, BlogRevisionService>();
        services.AddScoped<IBlogBlockService, BlogBlockService>();
        services.AddScoped<IBlogSettingService, BlogSettingService>();
        services.AddScoped<IEInvoiceService, EInvoiceService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IEInvoiceProviderFactory, EInvoiceProviderFactory>();
        services.AddTransient<IEInvoiceProvider, MisaProvider>();
        services.AddTransient<IEInvoiceProvider, ViettelProvider>();

        return services;
    }
}
