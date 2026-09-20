using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever().HasColumnName("CustomerId");
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(50);
        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(c => c.UpdatedAt).IsRequired(false);

        builder.Ignore(c => c.DomainEvents);
        builder.Ignore(c => c.FullName);

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").IsRequired().HasMaxLength(255);
            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.OwnsOne(c => c.BillingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("BillingStreet").IsRequired().HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("BillingCity").IsRequired().HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("BillingState").IsRequired().HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("BillingPostalCode").IsRequired().HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("BillingCountry").IsRequired().HasMaxLength(100);
        });

        builder.OwnsOne(c => c.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("ShippingPostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100);
        });
    }
}
