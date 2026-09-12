using FluentValidation;
using Orders.Api.Orders.Application.Dtos;
using Orders.Api.Orders.Application.Interfaces;
using Orders.Api.Orders.Domain.Entities;
using Orders.Api.Orders.Domain.Interfaces;

namespace Orders.Api.Orders.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _ordersRepo;
    private readonly IValidator<CreateOrder> _validator;

    public OrderService(IOrderRepository todoRepo, IValidator<CreateOrder> validator)
    {
        _ordersRepo = todoRepo;
        _validator = validator;
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
}