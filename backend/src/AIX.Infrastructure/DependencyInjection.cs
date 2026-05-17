using Microsoft.Extensions.DependencyInjection;

namespace AIX.Infrastructure;

/// <summary>
/// Cross-cutting infrastructure composition. Bounded contexts register their own adapters.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
