using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer entity configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever().HasColumnName("CustomerId");
            entity.Property(c => c.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(c => c.LastName).IsRequired().HasMaxLength(50);
            entity.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(c => c.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(c => c.UpdatedAt).IsRequired(false);

            entity.OwnsOne(c => c.Email, email =>
            {
                email.Property(e => e.Value).HasColumnName("Email").IsRequired().HasMaxLength(255);
                email.HasIndex(e => e.Value).IsUnique();
            });

            entity.OwnsOne(c => c.BillingAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("BillingStreet").IsRequired().HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("BillingCity").IsRequired().HasMaxLength(100);
                address.Property(a => a.State).HasColumnName("BillingState").IsRequired().HasMaxLength(100);
                address.Property(a => a.PostalCode).HasColumnName("BillingPostalCode").IsRequired().HasMaxLength(20);
                address.Property(a => a.Country).HasColumnName("BillingCountry").IsRequired().HasMaxLength(100);
            });

            entity.OwnsOne(c => c.ShippingAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
                address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100);
                address.Property(a => a.PostalCode).HasColumnName("ShippingPostalCode").HasMaxLength(20);
                address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100);
            });

            entity.Ignore(c => c.FullName);
        });

        // Product entity configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever().HasColumnName("ProductId");
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.StockQuantity).IsRequired();
            entity.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(p => p.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(p => p.UpdatedAt).IsRequired(false);

            entity.OwnsOne(p => p.Sku, sku =>
            {
                sku.Property(s => s.Value).HasColumnName("Sku").IsRequired().HasMaxLength(50);
                sku.HasIndex(s => s.Value).IsUnique();
            });

            entity.OwnsOne(p => p.Price, price =>
            {
                price.Property(m => m.Amount).HasColumnName("Price").HasColumnType("decimal(18,2)").IsRequired();
                price.Property(m => m.Currency).HasColumnName("Currency").IsRequired().HasMaxLength(3);
            });
        });

        // Order entity configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).ValueGeneratedNever().HasColumnName("OrderId");
            entity.Property(o => o.CustomerId).IsRequired();
            entity.Property(o => o.Status).IsRequired().HasConversion<int>();
            entity.Property(o => o.OrderDate).IsRequired();
            entity.Property(o => o.ConfirmedAt).IsRequired(false);
            entity.Property(o => o.ShippedAt).IsRequired(false);
            entity.Property(o => o.CancelledAt).IsRequired(false);
            entity.Property(o => o.Notes).HasMaxLength(1000);

            entity.OwnsMany(o => o.Lines, line =>
            {
                line.ToTable("OrderLines");
                line.WithOwner().HasForeignKey("OrderId");
                line.HasKey(l => l.Id);
                line.Property(l => l.Id).ValueGeneratedNever().HasColumnName("OrderLineId");
                line.Property(l => l.ProductId).IsRequired();
                line.Property(l => l.ProductName).IsRequired().HasMaxLength(200);

                line.OwnsOne(l => l.UnitPrice, price =>
                {
                    price.Property(m => m.Amount).HasColumnName("UnitPrice").HasColumnType("decimal(18,2)").IsRequired();
                    price.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").IsRequired().HasMaxLength(3);
                });

                line.OwnsOne(l => l.Quantity, qty =>
                {
                    qty.Property(q => q.Value).HasColumnName("Quantity").IsRequired();
                });
            });

            entity.Navigation(o => o.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
