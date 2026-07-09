using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Core.Entities;

namespace FoodstoreApi.Usecase.Services;

public class PaymentSettingService(IPaymentSettingRepository repository, ITenantContext tenantContext) : IPaymentSettingService
{
    private readonly IPaymentSettingRepository _repository = repository;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<PaymentSetting?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetDefaultAsync(cancellationToken);
    }

    public async Task<List<PaymentSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetAllAsync(cancellationToken);
        return settings.ToList();
    }

    public async Task<PaymentSetting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<PaymentSetting> SaveAsync(PaymentSetting setting, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(setting.Id, cancellationToken);
        
        if (existing == null)
        {
            setting.BranchId = _tenantContext.BranchId ?? throw new InvalidOperationException("An active branch is required.");
            // New payment setting - if it's set as default, clear other defaults
            if (setting.IsDefault)
            {
                await _repository.ClearOtherDefaultsAsync(null, cancellationToken);
            }
            
            await _repository.AddAsync(setting, cancellationToken);
        }
        else
        {
            // Update existing - if setting as default, clear other defaults
            if (setting.IsDefault && !existing.IsDefault)
            {
                await _repository.ClearOtherDefaultsAsync(existing.Id, cancellationToken);
            }

            existing.BankId = setting.BankId;
            existing.BankAccount = setting.BankAccount;
            existing.BankName = setting.BankName;
            existing.BankAccountName = setting.BankAccountName;
            existing.Template = setting.Template;
            existing.IsActive = setting.IsActive;
            existing.IsDefault = setting.IsDefault;

            await _repository.UpdateAsync(existing, cancellationToken);
        }

        return setting;
    }

    public async Task SetDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByIdAsync(id, cancellationToken);
        if (setting == null)
        {
            throw new Exception($"Payment setting with ID {id} not found");
        }

        // Clear all other defaults
        await _repository.ClearOtherDefaultsAsync(id, cancellationToken);

        // Set this one as default
        setting.IsDefault = true;
        await _repository.UpdateAsync(setting, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByIdAsync(id, cancellationToken);
        if (setting == null)
        {
            throw new Exception($"Payment setting with ID {id} not found");
        }

        // Prevent deleting the default payment setting
        if (setting.IsDefault)
        {
            throw new Exception("Cannot delete the default payment setting. Please set another account as default first.");
        }

        await _repository.DeleteAsync(setting, cancellationToken);
    }
}
