using Microsoft.AspNetCore.Authorization;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Application.Finance;
using MoviePlatform.Application.Subscriptions;
namespace MoviePlatform.Api.Endpoints;

public static class FinanceEndpoints
{
    public static RouteGroupBuilder MapFinanceEndpoints(this RouteGroupBuilder api)
    {
        var finance = api.MapGroup("/admin/finance")
            .RequireAuthorization(AppPolicies.FinanceAccess)
            .RequireAuthorization("AdminMfa");

        finance.MapGet("/payments", async (string? q, int? take, IFinanceService financeService, CancellationToken ct) =>
            Results.Ok(await financeService.ListRefundablePaymentsAsync(q, take ?? 50, ct)));

        finance.MapGet("/refunds", async (string? q, int? take, IFinanceService financeService, CancellationToken ct) =>
            Results.Ok(await financeService.ListRefundsAsync(q, take ?? 50, ct)));

        finance.MapGet("/payments/export", async (string? q, IFinanceService financeService, CancellationToken ct) =>
        {
            var rows = await financeService.ListRefundablePaymentsAsync(q, 500, ct);
            var csv = "PaymentId,OrderId,Email,AmountPaise,Currency,Status,RazorpayPaymentId,UpdatedAt\n" +
                      string.Join('\n', rows.Select(r =>
                          $"{r.PaymentId},{r.OrderId},{r.CustomerEmail},{r.AmountMinorUnits},{r.Currency},{r.Status},{r.RazorpayPaymentId},{r.UpdatedAt:O}"));
            return Results.Text(csv, "text/csv");
        });

        api.MapPost("/admin/refunds", async (
            RefundRequestDto request,
            IRefundService refunds,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            try
            {
                var refund = await refunds.RequestRefundAsync(request, ctx.User, ct);
                return Results.Ok(refund);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .RequireAuthorization(AppPolicies.FinanceAccess)
            .RequireAuthorization("AdminMfa");

        return api;
    }
}
