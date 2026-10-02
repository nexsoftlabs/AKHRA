namespace MoviePlatform.Application.Subscriptions;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    long PriceMinorUnits,
    string Currency,
    string BillingInterval,
    int BillingPeriodMonths,
    string BillingLabel);

public record RefundRequestDto(Guid PaymentId, string? Reason, long? AmountMinorUnits);

public record RefundDto(
    Guid Id,
    Guid PaymentId,
    string Status,
    long AmountMinorUnits,
    string Currency,
    string? ProviderRefundId);
