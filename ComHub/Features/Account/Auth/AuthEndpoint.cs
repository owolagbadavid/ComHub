namespace ComHub.Features.Account.Auth;

using System.ComponentModel.DataAnnotations;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Interfaces;
using ComHub.Shared.Models;
using Microsoft.AspNetCore.Mvc;

public class AuthEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth")
            .WithTags("Auth")
            .WithCommonResponses([StatusCodes.Status400BadRequest]);

        auth.MapPost(
                "/register",
                async (RegisterRequest request, AuthHandler handler) =>
                {
                    var result = await new RegisterRequestValidator().ValidateAsync(request);
                    if (!result.IsValid)
                    {
                        throw new BadRequestException(result.Errors[0].ErrorMessage);
                    }

                    await handler.RegisterUserAsync(request);

                    return Results.Created();
                }
            )
            .Produces<Response>(StatusCodes.Status201Created);

        auth.MapPost(
                "/login",
                async (LoginRequest request, AuthHandler handler) =>
                {
                    var result = await new LoginRequestValidator().ValidateAsync(request);
                    if (!result.IsValid)
                    {
                        throw new BadRequestException(result.Errors[0].ErrorMessage);
                    }

                    return Results.Ok(await handler.LoginUserAsync(request));
                }
            )
            .Produces<Response>(StatusCodes.Status200OK);

        auth.MapPost(
                "/reset-password",
                async (
                    [FromQuery] [EmailAddress] string email,
                    [FromBody] ResetPasswordRequest request,
                    AuthHandler handler
                ) =>
                {
                    var result = await new ResetPasswordRequestValidator().ValidateAsync(request);
                    if (!result.IsValid)
                    {
                        throw new BadRequestException(result.Errors[0].ErrorMessage);
                    }
                    await handler.ResetPasswordAsync(email, request);

                    return Results.Ok();
                }
            )
            .Produces<Response>(StatusCodes.Status200OK);

        auth.MapPost(
                "/forgot-password",
                async (ForgotPasswordRequest request, AuthHandler handler) =>
                {
                    var result = await new ForgotPasswordRequestValidator().ValidateAsync(request);
                    if (!result.IsValid)
                    {
                        throw new BadRequestException(result.Errors[0].ErrorMessage);
                    }

                    await handler.ForgotPasswordAsync(request.Email);

                    return Results.Ok();
                }
            )
            .Produces<Response>(StatusCodes.Status200OK);
    }
}
