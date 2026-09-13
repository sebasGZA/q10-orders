using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Inventory.Worker.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Inventory.Worker.Stocks.Application.Dtos;
using Inventory.Worker.Stocks.Application.Services;
using Inventory.Worker.Stocks.Domain.Enums;

namespace Inventory.Worker;

public class Worker : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceProvider _services;
    private readonly ILogger<Worker> _logger;

    public Worker(IOptions<RabbitMqOptions> options, IServiceProvider services, ILogger<Worker> logger)
    {
        _options = options.Value;
        _services = services;
        _logger = logger;
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
                _logger.LogError(ex, "Connection failed to RabbitMQ. Retrying in 5s.");
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
            ClientProvidedName = "inventory-worker"
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(_options.OrderCreatedQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(_options.StockResultQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.BasicQosAsync(0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(json)
                          ?? throw new InvalidOperationException("Invalid OrderCreated event.");

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

                var outcome = await StockService.TryReserveAsync(db, evt.EventId, evt.Sku, evt.Quantity, stoppingToken);

                if (outcome == BookedOutcome.Reserved)
                {
                    await PublishAsync(channel, _options.StockResultQueue,
                        new StockReservedEvent(Guid.NewGuid(), evt.OrderId, evt.Sku, evt.Quantity, DateTime.UtcNow),
                        stoppingToken);
                }
                else if (outcome == BookedOutcome.Rejected)
                {
                    await PublishAsync(channel, _options.StockResultQueue,
                        new StockRejectedEvent(Guid.NewGuid(), evt.OrderId, evt.Sku, evt.Quantity, DateTime.UtcNow, "Without stock"),
                        stoppingToken);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order-created message. it is Enqueued.");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(_options.OrderCreatedQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    private static async Task PublishAsync<T>(IChannel channel, string queue, T evt, CancellationToken ct)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
        var props = new BasicProperties { Persistent = true };
        await channel.BasicPublishAsync(exchange: "", routingKey: queue, mandatory: false, basicProperties: props, body: body, cancellationToken: ct);
    }
}
