using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever().HasColumnName("AuditLogId");
        builder.Property(a => a.UserId).IsRequired(false);
        builder.Property(a => a.Username).HasMaxLength(50);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).HasMaxLength(100);
        builder.Property(a => a.Payload).HasMaxLength(4000);
        builder.Property(a => a.Success).IsRequired();
        builder.Property(a => a.ErrorMessage).HasMaxLength(2000);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.Property(a => a.DurationMs).IsRequired();
        builder.Property(a => a.OccurredOn).IsRequired();

        builder.Ignore(a => a.DomainEvents);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.OccurredOn);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
