using Application.Common;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly IDomainEventDispatcher? _domainEventDispatcher;
    private bool _isDispatching;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<User> Users { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        if (_domainEventDispatcher is not null && !_isDispatching)
        {
            _isDispatching = true;
            try
            {
                var entitiesWithEvents = ChangeTracker
                    .Entries<IHasDomainEvents>()
                    .Where(e => e.Entity.DomainEvents.Count > 0)
                    .Select(e => e.Entity)
                    .ToList();

                if (entitiesWithEvents.Count > 0)
                {
                    await _domainEventDispatcher.DispatchAsync(entitiesWithEvents, cancellationToken);
                }
            }
            finally
            {
                _isDispatching = false;
            }
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
