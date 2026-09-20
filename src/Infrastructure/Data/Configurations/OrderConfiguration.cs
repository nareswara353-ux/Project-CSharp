using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever().HasColumnName("OrderId");
        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.Status).IsRequired().HasConversion<int>();
        builder.Property(o => o.OrderDate).IsRequired();
        builder.Property(o => o.ConfirmedAt).IsRequired(false);
        builder.Property(o => o.ShippedAt).IsRequired(false);
        builder.Property(o => o.CancelledAt).IsRequired(false);
        builder.Property(o => o.Notes).HasMaxLength(1000);

        builder.Ignore(o => o.DomainEvents);

        builder.OwnsMany(o => o.Lines, line =>
        {
            line.ToTable("OrderLines");
            line.WithOwner().HasForeignKey("OrderId");
            line.HasKey(l => l.Id);
            line.Property(l => l.Id).ValueGeneratedNever().HasColumnName("OrderLineId");
            line.Property(l => l.ProductId).IsRequired();
            line.Property(l => l.ProductName).IsRequired().HasMaxLength(200);

            line.OwnsOne(l => l.UnitPrice, price =>
            {
                price.Property(m => m.Amount)
                    .HasColumnName("UnitPrice")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                price.Property(m => m.Currency)
                    .HasColumnName("UnitPriceCurrency")
                    .IsRequired()
                    .HasMaxLength(3);
            });

            line.OwnsOne(l => l.Quantity, qty =>
            {
                qty.Property(q => q.Value).HasColumnName("Quantity").IsRequired();
            });
        });

        builder.Navigation(o => o.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
    }
}
