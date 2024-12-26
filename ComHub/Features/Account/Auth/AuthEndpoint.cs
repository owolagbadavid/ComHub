namespace ComHub.Features.Account.Auth;

using System.ComponentModel.DataAnnotations;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Interfaces;
using ComHub.Shared.Models;
using ComHub.Shared.Services.Utils;
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
                    await HelperService.HandleValidation(new RegisterRequestValidator(), request);

                    await handler.RegisterUserAsync(request);

                    return Results.Created();
                }
            )
            .Produces<Response>(StatusCodes.Status201Created);

        auth.MapPost(
                "/login",
                async (LoginRequest request, AuthHandler handler) =>
                {
                    await HelperService.HandleValidation(new LoginRequestValidator(), request);

                    return Results.Ok(await handler.LoginUserAsync(request));
                }
            )
            .Produces<DataResponse<TokenData>>(StatusCodes.Status200OK);

        auth.MapPost(
                "/reset-password",
                async (
                    [FromQuery] [EmailAddress] string email,
                    [FromBody] ResetPasswordRequest request,
                    AuthHandler handler
                ) =>
                {
                    await HelperService.HandleValidation(
                        new ResetPasswordRequestValidator(),
                        request
                    );

                    await handler.ResetPasswordAsync(email, request);

                    return Results.Ok();
                }
            )
            .Produces<Response>(StatusCodes.Status200OK);

        auth.MapPost(
                "/forgot-password",
                async (ForgotPasswordRequest request, AuthHandler handler) =>
                {
                    await HelperService.HandleValidation(
                        new ForgotPasswordRequestValidator(),
                        request
                    );

                    await handler.ForgotPasswordAsync(request.Email);

                    return Results.Ok();
                }
            )
            .Produces<Response>(StatusCodes.Status200OK);
    }
}

public class TokenData
{
    public required string Token { get; set; }
}
