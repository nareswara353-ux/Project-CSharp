namespace Application.Common;

public interface ICacheInvalidation
{
    IEnumerable<string> CacheKeysToInvalidate { get; }
}
