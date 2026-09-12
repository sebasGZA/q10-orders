using Orders.Api.Orders.Domain.Entities;

namespace Orders.Api.Orders.Application.Dtos;

public class OrderResponse
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static OrderResponse FromEntity(Order order) => new()
    {
        Id = order.Id,
        ClientName = order.ClientName,
        Sku = order.Sku,
        Quantity = order.Quantity,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
    };
}
