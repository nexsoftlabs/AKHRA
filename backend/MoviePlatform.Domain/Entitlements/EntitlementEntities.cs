using MoviePlatform.Domain.Catalog;
using MoviePlatform.Domain.Common;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Identity;

namespace MoviePlatform.Domain.Entitlements;

public class Entitlement : AuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public EntitlementSource Source { get; set; }
    public EntitlementStatus Status { get; set; } = EntitlementStatus.Active;
    public DateTimeOffset? ExpiresAt { get; set; }
    public Guid? PurchaseId { get; set; }
    public Guid? SubscriptionId { get; set; }
}

public class SubscriptionPlan : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long PriceMinorUnits { get; set; }
    public string Currency { get; set; } = "INR";
    public BillingInterval BillingInterval { get; set; } = BillingInterval.Monthly;
    public bool IsActive { get; set; } = true;
    public int? GracePeriodDays { get; set; }
    public string? RazorpayPlanId { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

public class Subscription : AuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid PlanId { get; set; }
    public SubscriptionPlan Plan { get; set; } = null!;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    public DateTimeOffset? CurrentPeriodStart { get; set; }
    public DateTimeOffset? CurrentPeriodEnd { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public string? ProviderSubscriptionId { get; set; }
    public ICollection<SubscriptionBillingEvent> BillingEvents { get; set; } = new List<SubscriptionBillingEvent>();
}

public class SubscriptionBillingEvent : Entity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public string EventType { get; set; } = string.Empty;
    public string? ProviderEventId { get; set; }
    public long? AmountMinorUnits { get; set; }
    public string? Currency { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

public class PlaybackSession : Entity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string? IpAddress { get; set; }
}

public class WatchProgress : AuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public int PositionSeconds { get; set; }
    public int? DurationSeconds { get; set; }
    public DateTimeOffset LastWatchedAt { get; set; }
}
