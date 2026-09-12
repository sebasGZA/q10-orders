using Microsoft.AspNetCore.Mvc;
using Orders.Api.Orders.Application.Dtos;
using Orders.Api.Orders.Application.Interfaces;

namespace Orders.Api.Orders.Infrastructure.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrder orderDto, CancellationToken ct)
    {
        var order = await _orderService.CreateAsync(orderDto, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll(CancellationToken ct)
    {
        var orders = await _orderService.GetAllAsync(ct);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }
}
