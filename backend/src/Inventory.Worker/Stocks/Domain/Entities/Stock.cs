namespace Inventory.Worker.Stocks.Domain.Entities;

public class Stock
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
