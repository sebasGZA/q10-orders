using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Orders.Api.Orders.Application.Interfaces; 
using Orders.Api.Orders.Application.Dtos;
using Orders.Api.Data;
using Orders.Api.Orders.Application.Services;
using Orders.Api.Orders.Infrastructure.Repositores;
using Orders.Api.Orders.Application.Validators;
using Orders.Api.Orders.Domain.Entities;
using Orders.Api.Orders.Domain.Enums;   

namespace Orders.Api.Tests;

public class IdempotencyTests
{
    private class NoOpOrderEventPublisher : IOrderEventPublisher
    {
        public Task<bool> PublishOrderCreatedAsync(OrderCreatedEvent evt, CancellationToken ct = default)
            => Task.FromResult(true);
    }

    private static (OrdersDbContext db, OrderService service) CreateSut()
    {
        var db = new OrdersDbContext(new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var orderRepo = new OrderRepository(db);
        var processedStockRepo = new ProcessedStockRepository(db);
        var validator = new CreateOrderValidator();
        var logger = NullLogger<OrderService>.Instance;
        var publisher = new NoOpOrderEventPublisher();

        var service = new OrderService(orderRepo, validator, publisher, logger, processedStockRepo);

        return (db, service);
    }

    [Fact]
    public async Task Process_same_event_twice_does_not_change_status()
    {
        var (db, service) = CreateSut();

        var order = new Order { ClientName = "Ana", Sku = "ABC-01", Quantity = 2, Status = OrderStatus.Pending };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var eventId = Guid.NewGuid();

        var once = await service.ApplyStockResultAsync(eventId, order.Id, reason: true);

        order.Status = OrderStatus.Rejected;
        await db.SaveChangesAsync();

        var twice = await service.ApplyStockResultAsync(eventId, order.Id, reason: false);

        Assert.True(once);
        Assert.False(twice);

        var final = await db.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.Rejected, final!.Status);
    }
}