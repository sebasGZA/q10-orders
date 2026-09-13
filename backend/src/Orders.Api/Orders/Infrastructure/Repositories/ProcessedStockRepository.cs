using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Orders.Domain.Entities;
using Orders.Api.Orders.Domain.Interfaces;

namespace Orders.Api.Orders.Infrastructure.Repositores;

public class ProcessedStockRepository : IProcessedStockRepository
{
    private readonly OrdersDbContext _context;

    public ProcessedStockRepository(OrdersDbContext context) => _context = context;

    public async Task<ProcessedStock?> GetByIdAsync(Guid eventId, CancellationToken ct) =>
        await _context.ProcessedStocks.Where(item => item.EventId == eventId).FirstOrDefaultAsync(ct);

    public async Task AddAsync(ProcessedStock processedStock, CancellationToken ct)
    {
        await _context.ProcessedStocks.AddAsync(processedStock);
        await _context.SaveChangesAsync(ct);
    }
}
