using ModularMonolith.Modules.Identity.Application.DependencyInjection;
using ModularMonolith.Modules.Identity.Infrastructure.DependencyInjection;
using ModularMonolith.Shared.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Modules.Identity.Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services, Settings settings)
        {
            services.AddIdentityApplication();
            services.AddIdentityInfrastructure(settings);

            return services;
        }
    }
}
