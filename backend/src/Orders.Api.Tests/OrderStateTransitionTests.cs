using Xunit;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Orders.Domain.Entities;
using Orders.Api.Orders.Domain.Enums;
using Orders.Api.Orders.Application.Services;
using Orders.Api.Orders.Infrastructure.Repositores;
using Orders.Api.Orders.Application.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Orders.Api.Orders.Application.Interfaces;
using Orders.Api.Orders.Application.Dtos;

namespace Orders.Api.Tests;

public class OrderStateTransitionTests
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
    public async Task OrderStatus_change_to_Confirmed_with_stock()
    {
        var (db, service) = CreateSut();
        var order = new Order { ClientName = "Ana", Sku = "ABC-01", Quantity = 2, Status = OrderStatus.Pending };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        await service.ApplyStockResultAsync(Guid.NewGuid(), order.Id, reason: false);

        var updated = await db.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.Confirmed, updated!.Status);
    }

    [Fact]
    public async Task OrderStatus_change_to_Rejected_with_no_stock()
    {
        var (db, service) = CreateSut();

        var order = new Order { ClientName = "Ana", Sku = "ABC-03", Quantity = 2, Status = OrderStatus.Pending };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        await service.ApplyStockResultAsync(Guid.NewGuid(), order.Id, reason: true);

        var updated = await db.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.Rejected, updated!.Status);
    }
}
