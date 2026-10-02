namespace MoviePlatform.Application.Finance;

public record RefundHistoryDto(
    Guid RefundId,
    Guid PaymentId,
    string? CustomerEmail,
    long AmountMinorUnits,
    string Currency,
    string Status,
    string? Reason,
    DateTimeOffset CreatedAt);

public record RefundablePaymentDto(
    Guid PaymentId,
    Guid OrderId,
    string? CustomerEmail,
    long AmountMinorUnits,
    string Currency,
    string Status,
    string? RazorpayPaymentId,
    DateTimeOffset UpdatedAt);
