using ComHub.Shared.Services.Notifications.Email;
using ComHub.Shared.Services.Utils;

namespace Microsoft.AspNetCore.Builder;

public static class SharedServicesExtension
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IUserContext, UserContext>();
        return services;
    }
}
