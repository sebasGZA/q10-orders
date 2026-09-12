using Orders.Api.Orders.Domain.Entities;

namespace Orders.Api.Orders.Domain.Interfaces;

public interface IProcessedStockRepository
{
    Task<ProcessedStock?> GetByIdAsync(Guid id, CancellationToken ct);

    Task AddAsync(ProcessedStock processedStock, CancellationToken ct);
}