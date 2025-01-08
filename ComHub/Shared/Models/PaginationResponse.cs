namespace ComHub.Shared.Models;

public class PaginationResponse<TData>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public required IEnumerable<TData> Items { get; set; }
    public IDictionary<string, object>? Extras { get; set; }
}
