using FoodstoreApi.Usecase.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FoodstoreApi.Usecase.Services;

public class PaymentProviderFactory(IServiceProvider serviceProvider) : IPaymentProviderFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IPaymentProvider? GetProvider(string providerType)
    {
        var providers = _serviceProvider.GetServices<IPaymentProvider>();
        return providers.FirstOrDefault(p =>
            p.ProviderType.Equals(providerType, StringComparison.OrdinalIgnoreCase));
    }
}
