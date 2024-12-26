using System.Text.Json.Serialization;
using ComHub.Shared.Config;
using ComHub.Shared.Services.Notifications.Email;
using ComHub.Shared.Services.Utils;

namespace Microsoft.AspNetCore.Builder;

public static class SharedServicesExtension
{
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        ConfigurationManager configuration
    )
    {
        services.Configure<Config>(configuration.GetSection("AppConfig"));

        services.Configure<Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.AllowTrailingCommas = true;
            options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            options.SerializerOptions.DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingDefault;
        });

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}
