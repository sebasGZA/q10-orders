namespace Inventory.Worker.Stocks.Application.Dtos;

public record StockReservedEvent(Guid EventId, Guid OrderId, string Sku, int Quantity, DateTime CreatedAt);

