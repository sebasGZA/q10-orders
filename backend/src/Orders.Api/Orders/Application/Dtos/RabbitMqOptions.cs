namespace Orders.Api.Orders.Application.Dtos;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    public string OrderCreatedQueue { get; set; } = "order-created";
    public string StockResultQueue { get; set; } = "stock-result";
}
