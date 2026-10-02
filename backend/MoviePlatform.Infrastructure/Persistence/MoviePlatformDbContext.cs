using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Domain.Audit;
using MoviePlatform.Domain.Catalog;
using MoviePlatform.Domain.Commerce;
using MoviePlatform.Domain.Entitlements;
using MoviePlatform.Domain.Identity;
using MoviePlatform.Domain.Media;
using MoviePlatform.Domain.Notifications;

namespace MoviePlatform.Infrastructure.Persistence;

public class MoviePlatformDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public MoviePlatformDbContext(DbContextOptions<MoviePlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<PhoneVerificationChallenge> PhoneVerificationChallenges => Set<PhoneVerificationChallenge>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<MovieLicense> MovieLicenses => Set<MovieLicense>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<UploadSession> UploadSessions => Set<UploadSession>();
    public DbSet<EncodingJob> EncodingJobs => Set<EncodingJob>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentEvent> PaymentEvents => Set<PaymentEvent>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<Entitlement> Entitlements => Set<Entitlement>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionBillingEvent> SubscriptionBillingEvents => Set<SubscriptionBillingEvent>();
    public DbSet<PlaybackSession> PlaybackSessions => Set<PlaybackSession>();
    public DbSet<WatchProgress> WatchProgress => Set<WatchProgress>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(MoviePlatformDbContext).Assembly);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.PreferredTerritory).HasMaxLength(8);
            b.HasIndex(u => u.DeletedAt);
        });

        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("external_logins");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        builder.Entity<ApplicationRole>().ToTable("roles");
        builder.Entity<ApplicationUser>().ToTable("users");
    }
}
