using Microsoft.EntityFrameworkCore;
using Orders.Api.Orders.Domain.Entities;

namespace Orders.Api.Data;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();


    public static readonly string[] skuCatalog = { "ABC-01", "ABC-02", "ABC-03" };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.ClientName).IsRequired().HasMaxLength(200);
            e.Property(o => o.Sku).IsRequired().HasMaxLength(50);
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}