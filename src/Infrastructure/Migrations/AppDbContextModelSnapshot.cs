using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.10")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("CustomerId");

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.OwnsOne(e => e.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);
                email.HasIndex(e => e.Value).IsUnique();
            });

            entity.OwnsOne(e => e.BillingAddress, address =>
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

            entity.OwnsOne(e => e.ShippingAddress, address =>
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

            entity.Ignore(e => e.FullName);
        });
    }
}
