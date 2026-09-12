namespace Orders.Api.Orders.Application.Dtos;

public record OrderCreatedEvent(Guid EventId, Guid OrderId, string Sku, int Quantity, DateTime CreatedAt);
