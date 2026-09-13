using Orders.Api.Orders.Application.Dtos;

namespace Orders.Api.Orders.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrder dto, CancellationToken ct);

    Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<PaginationResponse<OrderResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct);

    Task<bool> ApplyStockResultAsync(Guid eventId, Guid orderId, bool reason, CancellationToken ct = default);
}