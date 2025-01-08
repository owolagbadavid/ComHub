using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace ComHub.Shared.Models;

public class PaginationRequest
{
    [FromQuery]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public int PageSize { get; set; } = 20;

    [FromQuery]
    public string? SortColumn { get; set; }

    [FromQuery]
    public string SortOrder { get; set; } = "desc";
}
