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
                            if (string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
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
