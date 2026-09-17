using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ModularMonolith.Api.Middleware;
using ModularMonolith.Modules.Identity.Infrastructure.Persistence;
using ModularMonolith.Modules.Identity.Presentation;
using ModularMonolith.Shared;
using ModularMonolith.Shared.Configurations;
using ModularMonolith.Shared.Persistence;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, configuration) =>
        configuration.ReadFrom.Configuration(builder.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    builder.Services.AddControllers();
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ModularMonolith API",
            Version = "v1",
            Description = "Production-ready Clean Architecture Modular Monolith Project (.NET 10)"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter your JWT token directly.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", doc, null),
                []
            }
        });
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddOptions<Settings>()
        .Bind(builder.Configuration)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var settings = builder.Configuration.Get<Settings>()
        ?? throw new InvalidOperationException("Settings configuration is missing or invalid.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Jwt.Key)),
            ValidateIssuer = true,
            ValidIssuer = settings.Jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("IdentityDb")
        .AddDbContextCheck<SharedDbContext>("SharedDb");

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(settings.Cors.PolicyName, policy =>
        {
            if (settings.Cors.AllowedOrigins.Length > 0)
            {
                policy.WithOrigins(settings.Cors.AllowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
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
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ModularMonolith API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseCors(settings.Cors.PolicyName);
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

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