using FoodstoreApi.Core.Constants;
using FoodstoreApi.Usecase.DTOs.Order;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Core.Entities;

namespace FoodstoreApi.Usecase.Services;

public class OrderService(
    IOrderRepository orderRepo,
    IDiscountRepository discountRepo,
    IMenuItemRepository menuItemRepo,
    IComboRepository comboRepo,
    IAddonRepository addonRepo,
    ISourceRepository sourceRepo,
    IPaymentRepository paymentRepo,
    IPaymentSettingService paymentSettingService,
    ICustomerService customerService,
    ITenantContext tenantContext) : IOrderService
{
    private readonly IOrderRepository _orderRepo = orderRepo;
    private readonly IDiscountRepository _discountRepo = discountRepo;
    private readonly IMenuItemRepository _menuItemRepo = menuItemRepo;
    private readonly IComboRepository _comboRepo = comboRepo;
    private readonly IAddonRepository _addonRepo = addonRepo;
    private readonly ISourceRepository _sourceRepo = sourceRepo;
    private readonly IPaymentRepository _paymentRepo = paymentRepo;
    private readonly IPaymentSettingService _paymentSettingService = paymentSettingService;
    private readonly ICustomerService _customerService = customerService;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepo.GetAllAsync(cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, Guid employeeId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        if (!_tenantContext.IsSystem && !_tenantContext.IsPlatformOperator && !_tenantContext.BranchId.HasValue)
            throw new InvalidOperationException("An active branch is required to create an order.");

        var source = dto.SourceId.HasValue
            ? await _sourceRepo.GetByIdAsync(dto.SourceId.Value, cancellationToken)
            : null;
        if (dto.SourceId.HasValue && source is null)
            throw new InvalidOperationException("Order source was not found in the active branch.");
        
        var details = await CalculateOrderDetailsAsync(dto, cancellationToken);
        
        // Handle Discount Usage
        if (details.Discount != null)
        {
            details.Discount.UsedCount = (details.Discount.UsedCount ?? 0) + 1;
            await _discountRepo.UpdateAsync(details.Discount, cancellationToken);
        }
        
        var orderCode = await GenerateOrderCodeAsync(cancellationToken);
        var qrPaymentUrl = await GenerateQRPaymentUrlAsync(orderCode, details.TotalAmount, cancellationToken);

        var order = new Order
        {
            OrderCode = orderCode,
            SourceId = dto.SourceId,
            BranchId = source?.BranchId ?? _tenantContext.BranchId,
            EmployeeId = employeeId,
            DiscountId = details.Discount?.Id,
            CustomerId = dto.CustomerId,
            SubtotalAmount = details.Subtotal,
            DiscountAmount = details.DiscountAmount,
            VatAmount = details.VatAmount,
            TotalAmount = details.TotalAmount,
            QrPaymentUrl = qrPaymentUrl,
            Status = OrderStatus.Pending,
            Note = dto.Note,

            OrderItems = details.OrderItems,
            OrderStatusHistories = new List<OrderStatusHistory>
            {
                new OrderStatusHistory
                {
                    FromStatus = null,
                    ToStatus = OrderStatus.Pending,
                    ChangedBy = employeeId,
                    ChangedAt = now,
                    Note = "Order created"
                }
            }
        };

        await _orderRepo.CreateAsync(order, cancellationToken);
        var result = await _orderRepo.GetByIdAsync(order.Id, cancellationToken);
        return MapToDto(result!);
    }

    public async Task<OrderDto> UpdateAsync(Guid id, CreateOrderDto dto, Guid employeeId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
        if (order == null) throw new Exception("Order not found");
        if (order.Status != OrderStatus.Pending) throw new Exception($"Cannot update order in status {order.Status}");

        Source? source = null;
        if (dto.SourceId != order.SourceId && dto.SourceId.HasValue)
        {
            source = await _sourceRepo.GetByIdAsync(dto.SourceId.Value, cancellationToken);
            if (source is null)
                throw new InvalidOperationException("Order source was not found in the active branch.");
        }

        var now = DateTime.UtcNow;
        var details = await CalculateOrderDetailsAsync(dto, cancellationToken);
        
        // Handle Discount Usage Update
        if (order.DiscountId != details.Discount?.Id)
        {
             // Decrement old
             if (order.Discount != null)
             {
                  order.Discount.UsedCount = Math.Max(0, (order.Discount.UsedCount ?? 0) - 1);
                  await _discountRepo.UpdateAsync(order.Discount, cancellationToken);
             }
             
             // Increment new
             if (details.Discount != null)
             {
                  details.Discount.UsedCount = (details.Discount.UsedCount ?? 0) + 1;
                  await _discountRepo.UpdateAsync(details.Discount, cancellationToken);
             }
        }
        
        var qrPaymentUrl = await GenerateQRPaymentUrlAsync(order.OrderCode, details.TotalAmount, cancellationToken);
        
        // Update Fields
        order.SourceId = dto.SourceId;
        if (source is not null)
            order.BranchId = source.BranchId;
        order.CustomerId = dto.CustomerId;
        order.Note = dto.Note;
        
        order.DiscountId = details.Discount?.Id;
        order.SubtotalAmount = details.Subtotal;
        order.DiscountAmount = details.DiscountAmount;
        order.VatAmount = details.VatAmount;
        order.TotalAmount = details.TotalAmount;
        order.QrPaymentUrl = qrPaymentUrl;

        order.OrderItems?.Clear();
        if (order.OrderItems == null) order.OrderItems = new List<OrderItem>();
        
        foreach (var item in details.OrderItems)
        {
            order.OrderItems.Add(item);
        }
        
        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = order.Status,
            ToStatus = order.Status,
            ChangedBy = employeeId,
            ChangedAt = now,
            Note = "Order updated"
        };
        await _orderRepo.AddHistoryAsync(history, cancellationToken);

        await _orderRepo.UpdateAsync(order, cancellationToken);
        
        return MapToDto(order);
    }
    
    private async Task<(List<OrderItem> OrderItems, decimal Subtotal, decimal DiscountAmount, decimal VatAmount, decimal TotalAmount, Discount? Discount)> CalculateOrderDetailsAsync(CreateOrderDto dto, CancellationToken cancellationToken)
    {
        Discount? discount = null;
        decimal discountAmount = 0;
        var now = DateTime.UtcNow;
        
        if (!string.IsNullOrWhiteSpace(dto.DiscountCode))
        {
            discount = await _discountRepo.GetByCodeAsync(dto.DiscountCode, cancellationToken);
            
            if (discount != null && discount.IsActive == true)
            {
                if (discount.StartDate.HasValue && now < discount.StartDate.Value)
                    throw new Exception("Discount not yet valid");
                if (discount.EndDate.HasValue && now > discount.EndDate.Value)
                    throw new Exception("Discount expired");
                if (discount.UsageLimit.HasValue && (discount.UsedCount ?? 0) >= discount.UsageLimit.Value)
                    throw new Exception("Discount usage limit reached");
            }
            else
            {
                discount = null;
            }
        }

        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var itemDto in dto.Items)
        {
            decimal unitPrice = 0;
            string itemName = "";
            Guid? menuItemId = itemDto.MenuItemId;
            Guid? comboId = itemDto.ComboId;

            if (menuItemId.HasValue)
            {
                var menuItem = await _menuItemRepo.GetByIdAsync(menuItemId.Value, cancellationToken);
                if (menuItem == null) throw new Exception($"Menu item {menuItemId} not found");
                
                unitPrice = menuItem.Price;
                itemName = menuItem.Name;
            }
            else if (comboId.HasValue)
            {
                var combo = await _comboRepo.GetByIdAsync(comboId.Value, cancellationToken);
                if (combo == null) throw new Exception($"Combo {comboId} not found");
                
                unitPrice = combo.ComboPrice;
                itemName = combo.Name;
            }
            else
            {
                throw new Exception("Order item must be either a menu item or a combo");
            }

            var itemSubtotal = unitPrice * itemDto.Quantity;

            decimal addonTotal = 0;
            var orderItemAddons = new List<OrderItemAddon>();

            if (itemDto.Addons != null)
            {
                foreach (var addonDto in itemDto.Addons)
                {
                    var addon = await _addonRepo.GetByIdAsync(addonDto.AddonId, cancellationToken);
                    if (addon == null) throw new Exception($"Addon {addonDto.AddonId} not found");

                    var addonPrice = addon.Price;
                    var addonSubtotal = addonPrice * addonDto.Quantity;
                    addonTotal += addonSubtotal * itemDto.Quantity;

                    orderItemAddons.Add(new OrderItemAddon
                    {
                        AddonId = addonDto.AddonId,
                        AddonName = addon.Name,
                        Quantity = addonDto.Quantity,
                        UnitPrice = addonPrice,
                        TotalPrice = addonSubtotal,

                    });
                }
            }

            var orderItem = new OrderItem
            {
                MenuItemId = menuItemId,
                ComboId = comboId,
                MenuItemName = itemName,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = itemSubtotal + addonTotal,
                Note = itemDto.Note,

                OrderItemAddons = orderItemAddons
            };

            orderItems.Add(orderItem);
            subtotal += orderItem.TotalPrice;
        }

        if (discount != null)
        {
            if (discount.MinOrderAmount.HasValue && subtotal < discount.MinOrderAmount.Value)
                throw new Exception($"Minimum order amount is {discount.MinOrderAmount.Value:N0} VND");

            if (discount.Type == "percent")
            {
                discountAmount = subtotal * discount.Value / 100;
                if (discount.MaxDiscountAmount.HasValue)
                    discountAmount = Math.Min(discountAmount, discount.MaxDiscountAmount.Value);
            }
            else if (discount.Type == "amount")
            {
                discountAmount = discount.Value;
            }
        }

        var vatAmount = subtotal * 0.10m;
        var totalAmount = subtotal - discountAmount + vatAmount;
        
        return (orderItems, subtotal, discountAmount, vatAmount, totalAmount, discount);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid? employeeId = null, string? paymentMethod = null, decimal? paymentAmount = null, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
        if (order == null) return false;

        var oldStatus = order.Status;
        var now = DateTime.UtcNow;

        order.Status = status;

        if (status == OrderStatus.Paid)
        {
            order.PaidAt = now;
            
            if (!string.IsNullOrEmpty(paymentMethod) && paymentAmount.HasValue)
            {
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = paymentAmount.Value,
                    Method = paymentMethod,
                    Status = "success",
                    PaidAt = now,

                };
                await _paymentRepo.AddAsync(payment, cancellationToken);
            }



            // Loyalty Points Accumulation (1 point per 1000 VND)
            if (order.CustomerId.HasValue && order.TotalAmount.HasValue)
            {
                var points = (int)(order.TotalAmount.Value / 1000);
                await _customerService.AddPointsAsync(order.CustomerId.Value, points);
            }
        }

        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = oldStatus,
            ToStatus = status,
            ChangedBy = employeeId,
            ChangedAt = now,
            Note = $"Status changed from {oldStatus} to {status}"
        };
        
        await _orderRepo.AddHistoryAsync(history, cancellationToken);

        return await _orderRepo.UpdateAsync(order, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _orderRepo.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<OrderDto>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepo.GetByStatusAsync(status, cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepo.GetByDateRangeAsync(fromDate, toDate, cancellationToken);
        return orders.Select(MapToDto);
    }

    private async Task<string> GenerateOrderCodeAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"ORD-{today}";
        var lastOrder = await _orderRepo.GetLastOrderByCodePrefixAsync(prefix, cancellationToken);

        int nextNumber = 1;
        if (lastOrder != null)
        {
            var lastNumber = lastOrder.OrderCode.Split('-').Last();
            if (int.TryParse(lastNumber, out int num))
                nextNumber = num + 1;
        }

        return $"ORD-{today}-{nextNumber:D4}";
    }

    private async Task<string> GenerateQRPaymentUrlAsync(string orderCode, decimal amount, CancellationToken cancellationToken)
    {
        var paymentConfig = await _paymentSettingService.GetAsync(cancellationToken);

        var bankId = !string.IsNullOrWhiteSpace(paymentConfig?.BankId) ? paymentConfig.BankId : "970415";
        var accountNo = !string.IsNullOrWhiteSpace(paymentConfig?.BankAccount) ? paymentConfig.BankAccount : "0901234567";
        var template = !string.IsNullOrWhiteSpace(paymentConfig?.Template) ? paymentConfig.Template : "compact2";
        var accountName = Uri.EscapeDataString(paymentConfig?.BankAccountName ?? paymentConfig?.BankName ?? "");
        
        return $"https://api.vietqr.io/image/{bankId}-{accountNo}-{template}.jpg?amount={amount:F0}&addInfo={orderCode}&accountName={accountName}";
    }

    public async Task<OrderDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentDto dto, Guid? employeeId = null, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, cancellationToken);
        if (order == null) throw new Exception("Order not found");
        if (order.Status != OrderStatus.Pending) throw new Exception($"Cannot process payment. Order status is {order.Status}");

        var now = DateTime.UtcNow;
        var totalAmount = order.TotalAmount ?? 0;

        order.Status = OrderStatus.Paid;
        order.PaidAt = now;

        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = totalAmount,
            Method = dto.PaymentMethod,
            Status = "success",
            PaidAt = now,

        };
        await _paymentRepo.AddAsync(payment, cancellationToken);

        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = OrderStatus.Pending,
            ToStatus = OrderStatus.Paid,
            ChangedBy = employeeId,
            ChangedAt = now,
            Note = $"Payment processed via {dto.PaymentMethod}"
        };
        await _orderRepo.AddHistoryAsync(history, cancellationToken);



        // Loyalty Points Accumulation (1 point per 1000 VND)
        if (order.CustomerId.HasValue)
        {
            var points = (int)(totalAmount / 1000);
            await _customerService.AddPointsAsync(order.CustomerId.Value, points);
        }

        await _orderRepo.UpdateAsync(order, cancellationToken);

        var result = await _orderRepo.GetByIdAsync(orderId, cancellationToken);
        return MapToDto(result!);
    }

    public async Task<IEnumerable<OrderDto>> GetPaidOrdersForKitchenAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepo.GetPaidOrdersForKitchenAsync(cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<bool> UpdateKitchenStatusAsync(Guid orderId, string status, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, cancellationToken);
        if (order == null) return false;

        var validTransitions = OrderStatus.KitchenTransitions;

        var currentStatus = order.Status ?? OrderStatus.Pending;
        if (!validTransitions.ContainsKey(currentStatus) || 
            !validTransitions[currentStatus].Contains(status))
        {
            throw new Exception($"Invalid status transition from {currentStatus} to {status}");
        }

        var now = DateTime.UtcNow;
        var oldStatus = order.Status;
        order.Status = status;

        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = oldStatus,
            ToStatus = status,
            ChangedAt = now,
            Note = $"Kitchen updated status to {status}"
        };
        await _orderRepo.AddHistoryAsync(history, cancellationToken);
        
        return await _orderRepo.UpdateAsync(order, cancellationToken);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            SourceId = order.SourceId,
            SourceName = order.Source?.Name,
            EmployeeId = order.EmployeeId,
            EmployeeName = order.Employee?.User?.Name ?? order.Employee?.User?.UserName ?? "",
            DiscountId = order.DiscountId,
            DiscountCode = order.Discount?.Code,
            CustomerName = order.Customer?.User?.Name,
            CustomerPhone = order.Customer?.Phone,
            SubtotalAmount = order.SubtotalAmount ?? 0m,
            DiscountAmount = order.DiscountAmount ?? 0m,
            VatAmount = order.VatAmount ?? 0m,
            TotalAmount = order.TotalAmount ?? 0m,
            QrPaymentUrl = order.QrPaymentUrl,
            PaidAt = order.PaidAt,
            Status = order.Status ?? OrderStatus.Pending,
            Note = order.Note,
            PrintedAt = order.PrintedAt,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            CreatedBy = order.CreatedBy,
            UpdatedBy = order.UpdatedBy,
            Items = order.OrderItems?.Select(i => new OrderItemDto
            {
                Id = i.Id,
                MenuItemId = i.MenuItemId,
                MenuItemName = i.MenuItemName ?? "",
                ComboId = i.ComboId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Note = i.Note,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                Addons = i.OrderItemAddons?.Select(a => new OrderItemAddonDto
                {
                    Id = a.Id,
                    AddonId = a.AddonId,
                    AddonName = a.AddonName ?? "",
                    Quantity = a.Quantity ?? 0,
                    UnitPrice = a.UnitPrice,
                    TotalPrice = a.TotalPrice,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList() ?? new List<OrderItemAddonDto>()
            }).ToList() ?? new List<OrderItemDto>(),
            History = order.OrderStatusHistories?.Select(h => new OrderStatusHistoryDto
            {
                Id = h.Id,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                ChangedBy = h.ChangedBy,
                ChangedByName = h.ChangedByNavigation?.User?.Name ?? h.ChangedByNavigation?.User?.UserName,
                ChangedAt = h.ChangedAt,
                Note = h.Note
            }).OrderByDescending(h => h.ChangedAt).ToList() ?? new List<OrderStatusHistoryDto>()
        };
    }

    public async Task<(IEnumerable<OrderDto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _orderRepo.GetPagedAsync(page, pageSize, fromDate, toDate, cancellationToken);
        return (items.Select(MapToDto), totalCount);
    }
}
