using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoviePlatform.Application.Catalog;
using MoviePlatform.Application.Commerce;
using MoviePlatform.Application.Identity;
using MoviePlatform.Application.Media;
using MoviePlatform.Infrastructure.Catalog;
using MoviePlatform.Infrastructure.Commerce;
using MoviePlatform.Infrastructure.Media;
using MoviePlatform.Infrastructure.Subscriptions;
using MoviePlatform.Application.Subscriptions;
using MoviePlatform.Application.Finance;
using MoviePlatform.Infrastructure.Finance;
using MoviePlatform.Domain.Identity;
using MoviePlatform.Infrastructure.Identity;
using MoviePlatform.Infrastructure.Persistence;
using MoviePlatform.Infrastructure.Security;

namespace MoviePlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddDbContext<MoviePlatformDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(MoviePlatformDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(3);
                })
                .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

        var crossOriginSpa = environment is not null && CrossSiteCookiePolicy.IsCrossOriginSpa(environment);

        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        if (environment is not null)
        {
            services.PostConfigure<AuthOptions>(o => o.CrossOriginSpa = crossOriginSpa);
        }
        services.Configure<RazorpayOptions>(configuration.GetSection(RazorpayOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<SmsOptions>(configuration.GetSection(SmsOptions.SectionName));
        services.Configure<MediaStorageOptions>(configuration.GetSection(MediaStorageOptions.SectionName));
        services.Configure<AdminMfaOptions>(configuration.GetSection(AdminMfaOptions.SectionName));

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<MoviePlatformDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "mp.session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
            CrossSiteCookiePolicy.Apply(options.Cookie, crossOriginSpa);
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(
                configuration.GetValue<int?>("Auth:AccessCookieHours") ?? 8);
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddHttpClient<IRazorpayApiClient, RazorpayApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.razorpay.com/v1/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IAdminCatalogService, AdminCatalogService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IMediaUploadService, MediaUploadService>();
        services.AddScoped<IPlaybackService, PlaybackService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IRefundService, RefundService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<Application.Admin.IAdminDashboardService, Admin.AdminDashboardService>();
        services.AddSingleton<ICloudFrontUrlSigner, CloudFrontUrlSigner>();
        services.AddSingleton<S3MediaStorage>();
        services.AddSingleton<S3ObjectStorage>();
        services.AddSingleton<IS3ObjectStorage>(sp => sp.GetRequiredService<S3ObjectStorage>());
        services.AddSingleton<IMediaConvertEncodingService, MediaConvertEncodingService>();
        services.AddScoped<IHlsStreamService, HlsStreamService>();
        services.AddSingleton<LocalMediaBlobStorage>();
        services.AddSingleton<IMediaBlobStorage>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MediaStorageOptions>>().Value;
            return opts.UseS3
                ? sp.GetRequiredService<S3MediaStorage>()
                : sp.GetRequiredService<LocalMediaBlobStorage>();
        });
        services.AddScoped<AdminMfaService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpAuthService, OtpAuthService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddSingleton<IGoogleIdTokenValidator, GoogleIdTokenValidator>();
        services.AddSingleton<DevSmsSender>();
        services.AddSingleton<DevEmailSender>();
        services.AddSingleton<SmtpEmailSender>();
        services.AddSingleton<TwilioSmsSender>();
        services.AddSingleton<ISmsSender, CompositeSmsSender>();
        services.AddSingleton<IEmailSender, CompositeEmailSender>();
        services.AddHttpClient<TwilioSmsSender>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "movieplatform:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
