using Microsoft.EntityFrameworkCore;
using Inventory.Worker.Stocks.Domain.Entities;

namespace Inventory.Worker.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<ProcessedOrder> ProcessedOrder => Set<ProcessedOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>(e =>
        {
            e.HasKey(s => s.Sku);
        });

        modelBuilder.Entity<ProcessedOrder>(e =>
        {
            e.HasKey(p => p.EventId);
        });

        modelBuilder.Entity<Stock>().HasData(
            new Stock { Sku = "ABC-01", Quantity = 100 },
            new Stock { Sku = "ABC-02", Quantity = 50 },
            new Stock { Sku = "ABC-03", Quantity = 0 }
        );
    }
}
