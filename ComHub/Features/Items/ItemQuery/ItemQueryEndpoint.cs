namespace ComHub.Features.Items.ItemQuery;

using System.ComponentModel.DataAnnotations;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Shared.Interfaces;
using ComHub.Shared.Models;
using ComHub.Shared.Services.Utils;
using FluentValidation;
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
                    [FromQuery] string sortOrder = "desc"
                ) =>
                {
                    var request = new PaginationRequest
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        SortColumn = sortColumn,
                        SortOrder = sortOrder,
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
            .Produces<DataResponse<SearchItemModel>>(StatusCodes.Status200OK);
    }
}
