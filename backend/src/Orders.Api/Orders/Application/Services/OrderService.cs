using FluentValidation;
using Orders.Api.Orders.Application.Dtos;
using Orders.Api.Orders.Application.Interfaces;
using Orders.Api.Orders.Domain.Entities;
using Orders.Api.Orders.Domain.Enums;
using Orders.Api.Orders.Domain.Interfaces;

namespace Orders.Api.Orders.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _ordersRepo;
    private readonly IValidator<CreateOrder> _validator;
    private readonly IOrderEventPublisher _publisher;
    private readonly ILogger<OrderService> _logger;
    private readonly IProcessedStockRepository _processedStockRepo;

    public OrderService(
        IOrderRepository todoRepo,
        IValidator<CreateOrder> validator,
        IOrderEventPublisher publisher,
        ILogger<OrderService> logger,
        IProcessedStockRepository processedStockRepo
    )
    {
        _ordersRepo = todoRepo;
        _validator = validator;
        _publisher = publisher;
        _logger = logger;
        _processedStockRepo = processedStockRepo;
    }

    public async Task<OrderResponse> CreateAsync(CreateOrder dto, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ClientName = dto.ClientName,
            Sku = dto.Sku,
            Quantity = dto.Quantity,
        };

        await _ordersRepo.AddAsync(order, ct);

        var evt = new OrderCreatedEvent(Guid.NewGuid(), order.Id, order.Sku, order.Quantity, DateTime.UtcNow);
        var published = await _publisher.PublishOrderCreatedAsync(evt, ct);

        if (!published)
        {
            _logger.LogWarning("The order {OrderId} is pending: Cannot be published OrderCreated.", order.Id);
        }

        return OrderResponse.FromEntity(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var order = await _ordersRepo.GetByIdAsync(id, ct);
        return order is null ? null : OrderResponse.FromEntity(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllAsync(CancellationToken ct)
    {
        var orders = await _ordersRepo.GetAllAsync(ct);
        return orders.Select(o => OrderResponse.FromEntity(o));
    }

    public async Task<bool> ApplyStockResultAsync(
        Guid eventId,
        Guid orderId,
        bool booket,
        CancellationToken ct = default
    )
    {
        var processStock = await _processedStockRepo.GetByIdAsync(eventId, ct);
        if (processStock is not null)
        {
            return false;
        }

        var order = await _ordersRepo.GetByIdAsync(orderId, ct);
        if (order is null)
        {
            await _processedStockRepo.AddAsync(new ProcessedStock { EventId = eventId }, ct);
            return false;
        }

        if (order.Status == OrderStatus.Pending)
        {
            order.Status = booket ? OrderStatus.Confirmed : OrderStatus.Rejected;
        }

        await _processedStockRepo.AddAsync(new ProcessedStock { EventId = eventId }, ct);
        return true;
    }
}