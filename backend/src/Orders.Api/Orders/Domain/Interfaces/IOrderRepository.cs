using Orders.Api.Orders.Domain.Entities;

namespace Orders.Api.Orders.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<List<Order>> GetAllAsync(int page, int pageSize, CancellationToken ct);

    Task<int> GetAllCountAsync(CancellationToken ct);

    Task AddAsync(Order order, CancellationToken ct);
}