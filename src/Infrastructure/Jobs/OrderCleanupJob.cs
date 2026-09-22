using Application.Common;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

public class OrderCleanupJob
{
    private const int StaleThresholdDays = 7;

    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<OrderCleanupJob> _logger;

    public OrderCleanupJob(
        IOrderRepository orderRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<OrderCleanupJob> logger)
    {
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("OrderCleanupJob started");

        try
        {
            var drafts = await _orderRepository.GetByStatusAsync(OrderStatus.Draft);
            var cutoff = _dateTimeProvider.UtcNow.AddDays(-StaleThresholdDays);
            var cancelled = 0;

            foreach (var order in drafts.Where(o => o.OrderDate < cutoff))
            {
                order.Cancel();
                _orderRepository.Update(order);
                cancelled++;
            }

            if (cancelled > 0)
                await _orderRepository.SaveChangesAsync();

            _logger.LogInformation("OrderCleanupJob completed. Cancelled {Count} stale orders", cancelled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OrderCleanupJob failed");
            throw;
        }
    }
}
