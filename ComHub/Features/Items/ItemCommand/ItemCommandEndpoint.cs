namespace ComHub.Features.Items.ItemCommand;

using System.ComponentModel.DataAnnotations;
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

        item.MapPost(
                "/",
                async ([FromForm] CreateItemRequest request, ItemCommandHandler handler) =>
                {
                    await HelperService.HandleValidation(new CreateItemRequestValidator(), request);

                    return Results.Created("", await handler.AddEditItem(request));
                }
            )
            .Produces<DataResponse<int>>(StatusCodes.Status201Created);

        item.MapPut(
                "/{id}",
                async (int id, [FromForm] CreateItemRequest request, ItemCommandHandler handler) =>
                {
                    await HelperService.HandleValidation(new CreateItemRequestValidator(), request);

                    return Results.Ok(await handler.AddEditItem(request, id));
                }
            )
            .Produces<DataResponse<int>>(StatusCodes.Status200OK);
    }
}

public class CreateItemRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(1, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public string Brand { get; set; } = string.Empty;

    [Required]
    public ICollection<int> CategoryIds { get; set; } = [];

    [Required]
    public ICollection<IFormFile> Images { get; set; } = [];
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
        RuleFor(x => x.Images).Must(x => x.Count > 1).WithMessage("Minimum of 2 images required");
    }
}
