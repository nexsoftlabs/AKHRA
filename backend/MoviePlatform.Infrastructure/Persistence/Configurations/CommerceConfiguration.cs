using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoviePlatform.Domain.Commerce;
using MoviePlatform.Domain.Entitlements;

namespace MoviePlatform.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasIndex(o => o.CorrelationId);
        builder.Property(o => o.Currency).HasMaxLength(3).IsRequired();
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasIndex(p => p.RazorpayOrderId);
        builder.HasIndex(p => p.RazorpayPaymentId);
        builder.HasIndex(p => p.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL");
    }
}

public class PaymentEventConfiguration : IEntityTypeConfiguration<PaymentEvent>
{
    public void Configure(EntityTypeBuilder<PaymentEvent> builder)
    {
        builder.ToTable("payment_events");
        builder.HasIndex(e => e.ProviderEventId).IsUnique();
    }
}

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("purchases");
        builder.HasIndex(p => new { p.UserId, p.MovieId }).IsUnique();
    }
}

public class EntitlementConfiguration : IEntityTypeConfiguration<Entitlement>
{
    public void Configure(EntityTypeBuilder<Entitlement> builder)
    {
        builder.ToTable("entitlements");
        builder.HasIndex(e => new { e.UserId, e.MovieId, e.Source, e.Status });
        builder.HasIndex(e => new { e.UserId, e.MovieId })
            .IsUnique()
            .HasFilter("\"Status\" = 0");
    }
}

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");
        builder.HasIndex(p => p.Slug).IsUnique();
    }
}
