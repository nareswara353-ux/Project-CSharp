namespace Application.Common;

public class SortRequest
{
    public string? Field { get; set; }
    public SortingDirections Direction { get; set; } = SortingDirections.Ascending;

    public bool HasField => !string.IsNullOrWhiteSpace(Field);
}
