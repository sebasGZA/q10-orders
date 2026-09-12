using Orders.Api.Models.Enums;

namespace Orders.Api.Models.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ClientName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public int Quantity { set; get; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}