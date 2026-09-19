using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default);

    Task<Order?> GetWithLinesAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);
}
