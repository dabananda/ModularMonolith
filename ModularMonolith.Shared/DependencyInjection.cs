using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Behaviors;
using ModularMonolith.Shared.Configurations;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;
using ModularMonolith.Shared.Persistence;
using ModularMonolith.Shared.Services;
using System.Reflection;

namespace ModularMonolith.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedProject(this IServiceCollection services, Settings settings)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<AuditSaveChangesInterceptor>();

            services.AddDbContext<SharedDbContext>((sp, options) =>
            {
                options.UseNpgsql(settings.ConnectionStrings.PostgreSql);
                options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
            });

            services.AddScoped<ISender, Sender>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddDistributedMemoryCache();
            services.AddScoped<ICacheService, DistributedCacheService>();

            services.AddSingleton(settings);
            services.AddSingleton(settings.Cloudinary);

            services.AddScoped<IImageService, CloudinaryImageService>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}