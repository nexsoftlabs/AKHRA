namespace MoviePlatform.Application.Admin;

public record AdminDashboardStatsDto(
    int TotalUsers,
    int PublishedMovies,
    int DraftMovies,
    int ActiveSubscriptions,
    long RevenueCapturedMinorUnits30d,
    int PaymentsCaptured30d,
    int Refunds30d,
    IReadOnlyList<AdminRecentPaymentDto> RecentPayments);

public record AdminRecentPaymentDto(
    Guid PaymentId,
    string? CustomerEmail,
    long AmountMinorUnits,
    string Currency,
    string Status,
    DateTimeOffset UpdatedAt);
