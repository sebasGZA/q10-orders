namespace Orders.Api.Orders.Application.Dtos;

public record StockRejectedEvent(Guid EventId, Guid OrderId, string Sku, int Quantity, DateTime CreatedAt, string Reason);
