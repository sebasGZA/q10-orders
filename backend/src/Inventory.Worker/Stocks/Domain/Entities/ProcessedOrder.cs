namespace Inventory.Worker.Stocks.Domain.Entities;

public class ProcessedOrder
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
