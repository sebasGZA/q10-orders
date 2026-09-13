using Microsoft.EntityFrameworkCore;
using Inventory.Worker.Data;
using Inventory.Worker.Stocks.Domain.Enums;
using Inventory.Worker.Stocks.Domain.Entities;

namespace Inventory.Worker.Stocks.Application.Services;


public static class StockService
{
    public static async Task<BookedOutcome> TryReserveAsync(
        InventoryDbContext db,
        Guid eventId,
        string sku,
        int quantity,
        CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        if (await db.ProcessedOrders.FindAsync(new object[] { eventId }, ct) is not null)
        {
            return BookedOutcome.AlreadyProcessed;
        }

        var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Sku == sku, ct);

        BookedOutcome outcome;
        if (stock is not null && stock.Quantity >= quantity)
        {
            stock.Quantity -= quantity;
            outcome = BookedOutcome.Reserved;
        }
        else
        {
            outcome = BookedOutcome.Rejected;
        }

        db.ProcessedOrders.Add(new ProcessedOrder { EventId = eventId });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return outcome;
    }
}
