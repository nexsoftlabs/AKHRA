using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Commerce;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Worker;

/// <summary>Phase 5: logs payment totals for manual Razorpay settlement checks.</summary>
public sealed class ReconciliationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ReconciliationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<MoviePlatformDbContext>();
                var razorpay = scope.ServiceProvider.GetRequiredService<IRazorpayApiClient>();
                var razorpayOptions = scope.ServiceProvider.GetRequiredService<IOptions<RazorpayOptions>>().Value;
                var since = DateTimeOffset.UtcNow.AddDays(-1);

                var captured = await db.Payments.CountAsync(
                    p => p.Status == PaymentStatus.Captured && p.UpdatedAt >= since,
                    stoppingToken);
                var refunded = await db.Payments.CountAsync(
                    p => p.Status == PaymentStatus.Refunded && p.UpdatedAt >= since,
                    stoppingToken);
                var providerCaptured = razorpayOptions.IsConfigured
                    ? await razorpay.CountCapturedPaymentsSinceAsync(since, stoppingToken)
                    : 0;

                if (razorpayOptions.IsConfigured && providerCaptured != captured)
                {
                    logger.LogWarning(
                        "Reconciliation mismatch (24h): db_captured={DbCaptured} razorpay_captured={ProviderCaptured}",
                        captured,
                        providerCaptured);
                }
                else
                {
                    logger.LogInformation(
                        "Reconciliation snapshot (24h): captured={Captured} refunded={Refunded} razorpay={Provider}",
                        captured,
                        refunded,
                        providerCaptured);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reconciliation pass failed.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
