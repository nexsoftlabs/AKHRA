namespace MoviePlatform.Application.Finance;

public interface IFinanceService
{
    Task<IReadOnlyList<RefundablePaymentDto>> ListRefundablePaymentsAsync(string? search, int take, CancellationToken cancellationToken);

    Task<IReadOnlyList<RefundHistoryDto>> ListRefundsAsync(string? search, int take, CancellationToken cancellationToken);
}
