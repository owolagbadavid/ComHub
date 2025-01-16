using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace ComHub.Shared.Models;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortColumn { get; set; }
    public string SortOrder { get; set; } = "desc";
    public string? Search { get; set; }
}
