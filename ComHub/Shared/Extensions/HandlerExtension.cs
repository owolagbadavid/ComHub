using ComHub.Features.Account.Auth;
using ComHub.Features.Account.Profile;
using ComHub.Features.Items.ItemCommand;
using ComHub.Features.Items.ItemQuery;

namespace Microsoft.AspNetCore.Builder;

public static class HandlerExtension
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        services.AddScoped<AuthHandler>();
        services.AddScoped<ProfileHandler>();
        services.AddScoped<ItemCommandHandler>();
        services.AddScoped<ItemQueryHandler>();
        return services;
    }
}
