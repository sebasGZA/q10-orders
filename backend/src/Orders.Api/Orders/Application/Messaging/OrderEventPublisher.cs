using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Orders.Api.Orders.Application.Dtos;
using Orders.Api.Orders.Application.Interfaces;
using RabbitMQ.Client;

namespace Orders.Api.Orders.Application.Messaging;


public class OrderEventPublisher : IOrderEventPublisher
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<OrderEventPublisher> _logger;

    public OrderEventPublisher(IOptions<RabbitMqOptions> options, ILogger<OrderEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> PublishOrderCreatedAsync(OrderCreatedEvent evt, CancellationToken ct = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
            };

            await using var connection = await factory.CreateConnectionAsync("orders-api-publisher", ct);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

            await channel.QueueDeclareAsync(
                queue: _options.OrderCreatedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
            var props = new BasicProperties
            {
                Persistent = true,
                MessageId = evt.EventId.ToString()
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _options.OrderCreatedQueue,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed trying to publish order created fwith id {OrderId}. It is pending.", evt.OrderId);
            return false;
        }
    }
}
