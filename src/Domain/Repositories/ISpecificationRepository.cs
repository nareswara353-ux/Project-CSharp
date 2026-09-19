using Domain.Common;
using Domain.Specifications;

namespace Domain.Repositories;

public interface ISpecificationRepository<T> where T : Entity
{
    Task<IReadOnlyList<T>> ListAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);
}
