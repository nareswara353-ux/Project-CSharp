namespace Application.Common;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPaging<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize < 1 ? AppConstants.DefaultPageSize : pageSize;
        if (safePageSize > AppConstants.MaxPageSize)
            safePageSize = AppConstants.MaxPageSize;

        return query
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize);
    }

    public static IQueryable<T> ApplyConditional<T>(
        this IQueryable<T> query,
        bool condition,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
}
