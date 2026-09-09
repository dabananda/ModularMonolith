using ModularMonolith.Api.Middleware;
using ModularMonolith.Modules.Identity.Presentation;
using ModularMonolith.Shared;
using ModularMonolith.Shared.Configurations;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Starting application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, configuration) =>
        configuration.ReadFrom.Configuration(builder.Configuration)
                     .ReadFrom.Services(services).Enrich.FromLogContext());

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddHealthChecks();

    builder.Services.AddOptions<Settings>().Bind(builder.Configuration).ValidateDataAnnotations().ValidateOnStart();
    var settings = builder.Configuration.Get<Settings>() ?? throw new InvalidOperationException("Settings configuration is missing or invalid.");

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(settings.Cors.PolicyName, policy =>
        {
            policy.WithOrigins(settings.Cors.AllowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.Headers.RetryAfter = "10";
            await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
        };

        options.AddFixedWindowLimiter("auth", opt =>
        {
            opt.PermitLimit = 20;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
    });

    builder.Services
        .AddSharedProject(settings)
        .AddIdentityModule(settings);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseRateLimiter();
    app.UseCors(settings.Cors.PolicyName);
    app.UseStaticFiles();
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Application started successfully.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}