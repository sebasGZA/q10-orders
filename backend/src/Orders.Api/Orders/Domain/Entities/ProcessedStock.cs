namespace Orders.Api.Orders.Domain.Entities;

public class ProcessedStock
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
