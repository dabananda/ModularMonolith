using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Modules.Identity.Infrastructure.Persistence;
using ModularMonolith.Modules.Identity.Infrastructure.Repositories;
using ModularMonolith.Modules.Identity.Infrastructure.Security;
using ModularMonolith.Shared.Configurations;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Modules.Identity.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, Settings settings)
        {
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseNpgsql(settings.ConnectionStrings.PostgreSql);
                options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ISecureTokenGenerator, SecureTokenGenerator>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            return services;
        }
    }
}