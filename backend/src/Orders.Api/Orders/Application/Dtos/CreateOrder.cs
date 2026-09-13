namespace Orders.Api.Orders.Application.Dtos;

public class CreateOrder
{
    public string ClientName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
