using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Orders.Api.Orders.Application.Dtos;
using Orders.Api.Orders.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Orders.Api.Orders.Application.Messaging;

public class StockResultConsumer : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<StockResultConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public StockResultConsumer(
        IOptions<RabbitMqOptions> options,
        ILogger<StockResultConsumer> logger,
        IServiceScopeFactory scopeFactory
    )
    {
        _options = options.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunConsumerAsync(stoppingToken);
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Connection denied with RabbitMQ to consume stock-result. Retrying in 5s.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task RunConsumerAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            AutomaticRecoveryEnabled = true,
            ClientProvidedName = "orders-api-stock-result-consumer"
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(_options.StockResultQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.BasicQosAsync(0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var eventId = root.GetProperty("EventId").GetGuid();
                var orderId = root.GetProperty("OrderId").GetGuid();
                var reason = root.TryGetProperty("Reason", out JsonElement _);

                using var scope = _scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                await orderService.ApplyStockResultAsync(eventId, orderId, reason, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the stock-result message. it is enqueued.");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(_options.StockResultQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) { }
    }
}
