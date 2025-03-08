namespace ComHub.Features.Items.ItemQuery;

using ComHub.Shared.Interfaces;
using ComHub.Shared.Models;
using Microsoft.AspNetCore.Mvc;

public class ItemCommandEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var item = app.MapGroup("/items")
            .WithTags("Items")
            .RequireAuthorization()
            .WithCommonResponses([StatusCodes.Status401Unauthorized]);

        item.MapGet(
                "/",
                async (
                    ItemQueryHandler handler,
                    [FromQuery] int pageNumber = 1,
                    [FromQuery] int pageSize = 20,
                    [FromQuery] string? sortColumn = null,
                    [FromQuery] string sortOrder = "desc",
                    [FromQuery] string? search = null
                ) =>
                {
                    var request = new PaginationRequest
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        SortColumn = sortColumn,
                        SortOrder = sortOrder,
                        Search = search,
                    };

                    return Results.Ok(await handler.GetItems(request));
                }
            )
            .Produces<DataResponse<PaginationResponse<SearchItemModel>>>(StatusCodes.Status200OK);

        item.MapGet(
                "/{id:int}",
                async (ItemQueryHandler handler, int id) =>
                {
                    return Results.Ok(await handler.GetItem(id));
                }
            )
            .Produces<DataResponse<ItemModel>>(StatusCodes.Status200OK);

        item.MapGet(
                "/categories",
                async (
                    ItemQueryHandler handler,
                    [FromQuery] int pageNumber = 1,
                    [FromQuery] int pageSize = 20,
                    [FromQuery] string? sortColumn = null,
                    [FromQuery] string sortOrder = "desc",
                    [FromQuery] string? search = null
                ) =>
                {
                    var request = new PaginationRequest
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        SortColumn = sortColumn,
                        SortOrder = sortOrder,
                        Search = search,
                    };
                    return Results.Ok(await handler.GetCategories(request));
                }
            )
            .Produces<DataResponse<PaginationResponse<CategoryModel>>>(StatusCodes.Status200OK);
    }
}
