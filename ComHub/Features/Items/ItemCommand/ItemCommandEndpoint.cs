namespace ComHub.Features.Items.ItemCommand;

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ComHub.Features.Items.ItemQuery;
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
            .WithCommonResponses([StatusCodes.Status401Unauthorized])
            .DisableAntiforgery();

        item.MapPost(
                "/",
                async (
                    CreateItemRequest request,
                    ItemCommandHandler handler,
                    CancellationToken ct
                ) =>
                {
                    await HelperService.HandleValidation(new CreateItemRequestValidator(), request);

                    return Results.Created("", await handler.AddEditItem(request, ct: ct));
                }
            )
            .Produces<DataResponse<int>>(StatusCodes.Status201Created);

        item.MapPost(
                "/categories",
                async (
                    AddEditCategoriesRequest request,
                    ItemCommandHandler handler,
                    IMapper mapper,
                    CancellationToken ct
                ) =>
                {
                    await HelperService.HandleValidation(
                        new AddEditCategoriesRequestValidator(),
                        request
                    );
                    var categories = mapper.Map<List<Category>>(request.Categories);

                    return Results.Created("", await handler.AddEditCategories(categories, ct));
                }
            )
            .Produces<DataResponse<int>>(StatusCodes.Status201Created);

        item.MapDelete(
                "/categories",
                async (
                    [FromBody] List<int> categoryIds,
                    ItemCommandHandler handler,
                    CancellationToken ct
                ) =>
                {
                    await handler.DeleteCategories(categoryIds, ct);
                    return Results.Ok();
                }
            )
            .Produces<Response>(StatusCodes.Status200OK);

        item.MapPut(
                "/{id}",
                async (
                    int id,
                    CreateItemRequest request,
                    ItemCommandHandler handler,
                    CancellationToken ct
                ) =>
                {
                    await HelperService.HandleValidation(new CreateItemRequestValidator(), request);

                    return Results.Ok(await handler.AddEditItem(request, id, ct));
                }
            )
            .Produces<DataResponse<int>>(StatusCodes.Status200OK);

        item.MapDelete(
                "/{id}",
                async (int id, ItemCommandHandler handler, CancellationToken ct) =>
                {
                    await handler.DeleteItem(id, ct);
                    return Results.Ok();
                }
            )
            .Produces<Response>(StatusCodes.Status200OK);

        item.MapPatch(
                "/{id}/images",
                async (
                    int id,
                    [FromForm] IFormFileCollection images,
                    ItemCommandHandler handler,
                    CancellationToken ct
                ) =>
                {
                    return Results.Ok(await handler.AddItemImages(id, images, ct));
                }
            )
            .Produces<DataResponse<int>>(StatusCodes.Status200OK);

        item.MapDelete(
                "/{id}/images",
                async (
                    int id,
                    [FromBody] List<int> imageIds,
                    ItemCommandHandler handler,
                    CancellationToken ct
                ) =>
                {
                    await handler.DeleteItemImages(id, imageIds, ct);
                    return Results.Ok();
                }
            )
            .Produces<Response>(StatusCodes.Status200OK);
    }
}

public class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Brand).NotEmpty();
        RuleFor(x => x.CategoryIds)
            .Must(x => x.Count <= 5)
            .WithMessage("Maximum of 5 categories allowed");
        RuleForEach(x => x.CategoryIds).GreaterThan(0);
    }
}

public class AddEditCategoriesRequestValidator : AbstractValidator<AddEditCategoriesRequest>
{
    public AddEditCategoriesRequestValidator()
    {
        // RuleForEach(x => x.Categories).SetValidator(new CategoryModelValidator());
        RuleForEach(x => x.Categories).ChildRules(x => x.RuleFor(y => y.Name).NotEmpty());
    }
}

// public class CategoryModelValidator : AbstractValidator<CategoryModel>
// {
//     public CategoryModelValidator()
//     {
//         RuleFor(x => x.Name).NotEmpty();
//     }
// }
