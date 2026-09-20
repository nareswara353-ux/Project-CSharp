using Domain.Common;

namespace Application.Common;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IHasDomainEvents> entities, CancellationToken cancellationToken = default);
}
