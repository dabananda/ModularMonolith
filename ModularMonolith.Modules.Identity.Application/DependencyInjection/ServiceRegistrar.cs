using ModularMonolith.Shared.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Modules.Identity.Application.DependencyInjection;

internal static class ServiceRegistrar
{
    public static void Register(IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        var implementations = assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .ToArray();

        RegisterOpenGeneric(
            services,
            implementations,
            typeof(IRequestHandler<,>));

        RegisterOpenGeneric(
            services,
            implementations,
            typeof(IPipelineBehavior<,>));

        services.AddValidatorsFromAssembly(assembly);
    }

    private static void RegisterOpenGeneric(IServiceCollection services, IEnumerable<Type> implementations, Type openGenericType)
    {
        foreach (var implementation in implementations)
        {
            foreach (var service in implementation.GetInterfaces())
            {
                if (!service.IsGenericType)
                    continue;

                if (service.GetGenericTypeDefinition() != openGenericType)
                    continue;

                if (implementation.IsGenericTypeDefinition)
                {
                    services.AddScoped(service.GetGenericTypeDefinition(), implementation);
                }
                else
                {
                    services.AddScoped(service, implementation);
                }
            }
        }
    }
}