using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Modules.Identity.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        ServiceRegistrar.Register(services);

        return services;
    }
}