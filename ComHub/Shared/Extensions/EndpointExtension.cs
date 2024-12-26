using System.Reflection;
using ComHub.Shared.Interfaces;
using ComHub.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class EndpointExtensions
{
    public static void RegisterEndpoints(this IEndpointRouteBuilder app)
    {
        // Get the current assembly
        var assembly = Assembly.GetExecutingAssembly();

        // Find all types that implement IEndpoint
        var endpointTypes = assembly
            .GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in endpointTypes)
        {
            // Create an instance of the endpoint class
            var endpointInstance = Activator.CreateInstance(type) as IEndpoint;

            // Map the endpoints
            endpointInstance?.MapEndpoints(app);
        }
    }
}

public static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder WithCommonResponses(
        this RouteGroupBuilder group,
        List<int>? statusCodes = null
    )
    {
        foreach (var code in statusCodes ?? [])
        {
            if (code >= 400)
            {
                group.WithMetadata(new ProducesResponseTypeAttribute(typeof(ErrorResponse), code));
            }
            else
            {
                group.WithMetadata(new ProducesResponseTypeAttribute(typeof(Response), code));
            }
        }
        return group.WithMetadata(
            new ProducesResponseTypeAttribute(
                typeof(ErrorResponse),
                StatusCodes.Status500InternalServerError
            )
        );
    }
}
