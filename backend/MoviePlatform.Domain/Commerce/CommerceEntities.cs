using MoviePlatform.Domain.Catalog;
using MoviePlatform.Domain.Common;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Entitlements;
using MoviePlatform.Domain.Identity;

namespace MoviePlatform.Domain.Commerce;

public class Order : AuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public long TotalMinorUnits { get; set; }
    public string Currency { get; set; } = "INR";
    public string? CorrelationId { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class OrderItem : Entity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? MovieId { get; set; }
    public Movie? Movie { get; set; }
    public Guid? SubscriptionPlanId { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }
    public string Description { get; set; } = string.Empty;
    public long UnitPriceMinorUnits { get; set; }
    public int Quantity { get; set; } = 1;
}

public class Payment : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public long AmountMinorUnits { get; set; }
    public string Currency { get; set; } = "INR";
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? IdempotencyKey { get; set; }
    public ICollection<PaymentEvent> Events { get; set; } = new List<PaymentEvent>();
}

public class PaymentEvent : Entity
{
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public string ProviderEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string RawPayload { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; set; }
    public bool Processed { get; set; }
}

public class Purchase : AuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public long AmountPaidMinorUnits { get; set; }
    public string Currency { get; set; } = "INR";
}

public class Refund : AuditableEntity
{
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public RefundStatus Status { get; set; } = RefundStatus.Requested;
    public long AmountMinorUnits { get; set; }
    public string Currency { get; set; } = "INR";
    public string? Reason { get; set; }
    public Guid? RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? ProviderRefundId { get; set; }
}
