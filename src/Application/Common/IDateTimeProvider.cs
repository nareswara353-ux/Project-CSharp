namespace Application.Common;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Today { get; }
}
