using System.Text.Json.Serialization;
using Amazon.S3;
using ComHub.Infrastructure.Cloud;
using ComHub.Shared.Config;
using ComHub.Shared.Services.Notifications.Email;
using ComHub.Shared.Services.Utils;

namespace Microsoft.AspNetCore.Builder;

public static class SharedServicesExtension
{
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        ConfigurationManager configuration,
        Config appConfig
    )
    {
        var awsOptions = appConfig.CloudConfig;
        var s3Client = new AmazonS3Client(
            awsOptions.AccessKey,
            awsOptions.SecretKey,
            Amazon.RegionEndpoint.GetBySystemName(awsOptions.Region)
        );

        services.AddSingleton<IAmazonS3>(s3Client);
        services.AddSingleton<ICloudStorage, CloudStorage>();

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
