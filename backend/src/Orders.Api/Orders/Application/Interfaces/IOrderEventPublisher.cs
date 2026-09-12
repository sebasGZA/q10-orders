using Orders.Api.Orders.Application.Dtos;

namespace Orders.Api.Orders.Application.Interfaces;

public interface IOrderEventPublisher
{
    Task<bool> PublishOrderCreatedAsync(OrderCreatedEvent evt, CancellationToken ct = default);
}