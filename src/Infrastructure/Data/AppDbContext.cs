using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer entity configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .ValueGeneratedNever()
                .HasColumnName("CustomerId");

            entity.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(c => c.UpdatedAt)
                .IsRequired(false);

            // Email ValueObject conversion
            entity.OwnsOne(c => c.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                email.HasIndex(e => e.Value)
                    .IsUnique();
            });

            // BillingAddress ValueObject conversion
            entity.OwnsOne(c => c.BillingAddress, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("BillingStreet")
                    .IsRequired()
                    .HasMaxLength(200);

                address.Property(a => a.City)
                    .HasColumnName("BillingCity")
                    .IsRequired()
                    .HasMaxLength(100);

                address.Property(a => a.State)
                    .HasColumnName("BillingState")
                    .IsRequired()
                    .HasMaxLength(100);

                address.Property(a => a.PostalCode)
                    .HasColumnName("BillingPostalCode")
                    .IsRequired()
                    .HasMaxLength(20);

                address.Property(a => a.Country)
                    .HasColumnName("BillingCountry")
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // ShippingAddress ValueObject (nullable)
            entity.OwnsOne(c => c.ShippingAddress, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("ShippingStreet")
                    .HasMaxLength(200);

                address.Property(a => a.City)
                    .HasColumnName("ShippingCity")
                    .HasMaxLength(100);

                address.Property(a => a.State)
                    .HasColumnName("ShippingState")
                    .HasMaxLength(100);

                address.Property(a => a.PostalCode)
                    .HasColumnName("ShippingPostalCode")
                    .HasMaxLength(20);

                address.Property(a => a.Country)
                    .HasColumnName("ShippingCountry")
                    .HasMaxLength(100);
            });

            // Ignore FullName computed property
            entity.Ignore(c => c.FullName);
        });
    }
}
