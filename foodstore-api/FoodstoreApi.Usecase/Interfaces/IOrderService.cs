using FoodstoreApi.Usecase.DTOs.Order;

namespace FoodstoreApi.Usecase.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderReceiptDto?> GetReceiptAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, Guid employeeId, CancellationToken cancellationToken = default);
    Task<OrderDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentDto dto, Guid? employeeId = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid id, string status, Guid? employeeId = null, string? paymentMethod = null, decimal? paymentAmount = null, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateAsync(Guid id, CreateOrderDto dto, Guid employeeId, CancellationToken cancellationToken = default);
    Task<bool> UpdateKitchenStatusAsync(Guid orderId, string status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetByStatusAsync(string status, CancellationToken cancellationtoken = default);
    Task<IEnumerable<OrderDto>> GetPaidOrdersForKitchenAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<(IEnumerable<OrderDto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
}
