using ComHub.Features.Account.Auth;
using ComHub.Features.Account.Profile;
using ComHub.Features.Items.ItemCommand;

namespace Microsoft.AspNetCore.Builder;

public static class HandlerExtension
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        services.AddScoped<AuthHandler>();
        services.AddScoped<ProfileHandler>();
        services.AddScoped<ItemCommandHandler>();
        return services;
    }
}
