using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Orders.Domain.Entities;
using Orders.Api.Orders.Domain.Interfaces;

namespace Orders.Api.Orders.Infrastructure.Repositores;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context) => _context = context;

    public async Task<List<Order>> GetAllAsync(CancellationToken ct) =>
        await _context.Orders.OrderByDescending(o => o.CreatedAt).ToListAsync(ct);


    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _context.Orders.Where(item => item.Id == id).FirstOrDefaultAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync(ct);
    }
}
