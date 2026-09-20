namespace Application.Common;

public class FilterRequest
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }

    public bool HasSearch => !string.IsNullOrWhiteSpace(SearchTerm);
    public bool HasActiveFilter => IsActive.HasValue;
}
