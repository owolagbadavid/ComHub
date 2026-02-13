using System.Text;
using ComHub.Shared.Config;
using ComHub.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.AspNetCore.Builder;

public static class AuthExtension
{
    public static IServiceCollection RegisterAuth(this IServiceCollection services, Config config)
    {
        var jwtSettings = config.JwtSettings;

        services.AddHttpContextAccessor();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                (options) =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.Secret)
                        ),
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (
                                !string.IsNullOrWhiteSpace(accessToken)
                                && path.StartsWithSegments("/hub")
                            )
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = context =>
                        {
                            throw new UnauthorizedException("Invalid token");
                        },

                        OnForbidden = context =>
                        {
                            throw new ForbiddenException("Forbidden access");
                        },

                        OnChallenge = context =>
                        {
                            var hasAuthHeader = !string.IsNullOrEmpty(
                                context.Request.Headers["Authorization"]
                            );
                            var hasQueryToken = !string.IsNullOrWhiteSpace(
                                context.Request.Query["access_token"]
                            );

                            if (!hasAuthHeader && !hasQueryToken)
                            {
                                throw new UnauthorizedException("Authorization header is missing");
                            }

                            return Task.CompletedTask;
                        },
                    };
                }
            );

        services.AddAuthorization();

        return services;
    }

    public static IEndpointRouteBuilder RegisterAuth(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
