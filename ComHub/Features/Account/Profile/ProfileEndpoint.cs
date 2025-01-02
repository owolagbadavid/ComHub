namespace ComHub.Features.Account.Profile;

using System.ComponentModel.DataAnnotations;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Interfaces;
using ComHub.Shared.Models;
using ComHub.Shared.Services.Utils;
using FluentValidation;

public class ProfileEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var profile = app.MapGroup("/profile")
            .WithTags("Profile")
            .RequireAuthorization()
            .WithCommonResponses([StatusCodes.Status401Unauthorized]);

        profile
            .MapGet(
                "/",
                async (ProfileHandler handler, IUserContext user) =>
                {
                    var userId = user.UserId;

                    if (userId == 0)
                        throw new UnauthorizedException("User is not authorized");

                    return Results.Ok(await handler.GetProfileAsync(userId));
                }
            )
            .Produces<DataResponse<ProfileModel>>(StatusCodes.Status200OK);

        profile
            .MapPut(
                "/",
                async (PutProfileRequest request, ProfileHandler handler, IUserContext user) =>
                {
                    var userId = user.UserId;

                    if (userId == 0)
                        throw new UnauthorizedException("User is not authorized");

                    await HelperService.HandleValidation(new PutProfileRequestValidator(), request);

                    await handler.UpdateProfileAsync(userId, request);

                    return Results.Ok();
                }
            )
            .Produces<Response>(StatusCodes.Status200OK);
    }
}

public class PutProfileRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }

    [Url]
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }

    [Url]
    public string? Website { get; set; }
    public string? Location { get; set; }
}

public class PutProfileRequestValidator : AbstractValidator<PutProfileRequest>
{
    public PutProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.ProfilePicture)
            .Must(HelperService.BeAValidUrl)
            .When(x => x.ProfilePicture != null)
            .WithMessage("'ProfilePicture' Invalid URL");
        RuleFor(x => x.Website)
            .Must(HelperService.BeAValidUrl)
            .When(x => x.Website != null)
            .WithMessage("'Website' Invalid URL");
    }
}
