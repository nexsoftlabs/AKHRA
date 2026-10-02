namespace MoviePlatform.Application.Commerce;

public record CreateCheckoutResponse(
    Guid PaymentId,
    string RazorpayOrderId,
    string RazorpayKeyId,
    long AmountMinorUnits,
    string Currency,
    string MovieTitle,
    string? RazorpaySubscriptionId = null);

public record VerifyCheckoutRequest(
    string? RazorpayOrderId,
    string RazorpayPaymentId,
    string RazorpaySignature,
    string? RazorpaySubscriptionId = null);

public record VerifyCheckoutResponse(
    bool Success,
    string? Message,
    Guid? MovieId,
    bool AlreadyOwned);

public sealed class CheckoutResult
{
    public bool Succeeded { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public CreateCheckoutResponse? Checkout { get; init; }
    public VerifyCheckoutResponse? Verification { get; init; }

    public static CheckoutResult Ok(CreateCheckoutResponse checkout) =>
        new() { Succeeded = true, Checkout = checkout };

    public static CheckoutResult Verified(VerifyCheckoutResponse verification) =>
        new() { Succeeded = true, Verification = verification };

    public static CheckoutResult Fail(string code, string message) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message };
}
