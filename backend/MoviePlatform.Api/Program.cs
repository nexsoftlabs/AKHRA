using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using MoviePlatform.Api.Authorization;
using MoviePlatform.Api.Endpoints;
using MoviePlatform.Api.Security;
using MoviePlatform.Application;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Infrastructure;
using MoviePlatform.Infrastructure.Persistence;
using MoviePlatform.Infrastructure.Subscriptions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrWhiteSpace(port))
    {
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    }

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHostedService<SubscriptionRazorpayPlanSync>();
    builder.Services.AddSpaAntiforgery();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IAuthorizationHandler, AdminMfaHandler>();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminMfa", policy => policy.Requirements.Add(new AdminMfaRequirement()));
        options.AddPolicy(AppPolicies.AdminAccess, policy =>
            policy.RequireRole(AppRoles.PlatformAdmin, AppRoles.SuperAdmin));
        options.AddPolicy(AppPolicies.ContentManagement, policy =>
            policy.RequireRole(AppRoles.ContentManager, AppRoles.PlatformAdmin, AppRoles.SuperAdmin));
        options.AddPolicy(AppPolicies.FinanceAccess, policy =>
            policy.RequireRole(AppRoles.FinanceAdmin, AppRoles.PlatformAdmin, AppRoles.SuperAdmin));
        options.AddPolicy(AppPolicies.SupportAccess, policy =>
            policy.RequireRole(AppRoles.SupportAgent, AppRoles.PlatformAdmin, AppRoles.SuperAdmin));
        options.AddPolicy(AppPolicies.AdminPanel, policy =>
            policy.RequireRole(
                AppRoles.ContentManager,
                AppRoles.SupportAgent,
                AppRoles.FinanceAdmin,
                AppRoles.PlatformAdmin,
                AppRoles.SuperAdmin));
    });

    builder.Services.AddProblemDetails();
    builder.Services.AddOpenApi();

    var healthChecks = builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrWhiteSpace(redisConnection))
    {
        healthChecks.AddRedis(redisConnection);
    }

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

    builder.Services.AddCors(options =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
        options.AddPolicy("Frontend", policy =>
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    });

    var app = builder.Build();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        await DbInitializer.InitializeAsync(app.Services, app.Environment);
    }

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseApiAntiforgery();

    var api = app.MapGroup("/api/v1");
    api.MapAntiforgeryEndpoint();
    api.MapAuthEndpoints();
    api.MapMfaEndpoints();
    api.MapCatalogEndpoints();
    api.MapCheckoutEndpoints();
    api.MapAdminDashboardEndpoints();
    api.MapAdminCatalogEndpoints();
    api.MapAdminMediaEndpoints();
    api.MapPlaybackEndpoints();
    api.MapPlaybackStreamEndpoints();
    api.MapSubscriptionEndpoints();
    api.MapFinanceEndpoints();

    api.MapGet("/health", () => Results.Ok(new { status = "ok", service = "MoviePlatform.Api" }))
        .WithName("Health")
        .AllowAnonymous();

    api.MapGet("/version", () => Results.Ok(new
    {
        name = "MoviePlatform",
        version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.1.0",
        framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
    }))
        .WithName("Version")
        .AllowAnonymous();

    app.MapHealthChecks("/healthz");
    app.MapHealthChecks("/healthz/ready");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
