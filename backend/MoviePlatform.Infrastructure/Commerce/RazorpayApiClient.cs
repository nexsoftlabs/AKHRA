using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace MoviePlatform.Infrastructure.Commerce;

public interface IRazorpayApiClient
{
    Task<string> CreateOrderAsync(
        long amountMinorUnits,
        string currency,
        string receipt,
        IReadOnlyDictionary<string, string> notes,
        CancellationToken cancellationToken);

    Task<string?> GetPaymentStatusAsync(string razorpayPaymentId, CancellationToken cancellationToken);

    Task<string> CreateRefundAsync(string razorpayPaymentId, long amountMinorUnits, CancellationToken cancellationToken);

    Task<string> EnsurePlanAsync(
        string name,
        long amountMinorUnits,
        string period,
        int periodInterval,
        CancellationToken cancellationToken);

    Task<string> CreateSubscriptionAsync(string planId, int totalCount, CancellationToken cancellationToken);

    Task<int> CountCapturedPaymentsSinceAsync(DateTimeOffset since, CancellationToken cancellationToken);
}

public sealed class RazorpayApiClient(HttpClient httpClient, IOptions<RazorpayOptions> options) : IRazorpayApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<string> CreateOrderAsync(
        long amountMinorUnits,
        string currency,
        string receipt,
        IReadOnlyDictionary<string, string> notes,
        CancellationToken cancellationToken)
    {
        var razorpay = options.Value;
        if (!razorpay.IsConfigured)
        {
            throw new InvalidOperationException("Razorpay is not configured. Set Razorpay:KeyId and Razorpay:KeySecret.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "orders");
        request.Headers.Authorization = CreateBasicAuth(razorpay.KeyId, razorpay.KeySecret);
        request.Content = JsonContent.Create(
            new
            {
                amount = amountMinorUnits,
                currency,
                receipt,
                notes,
            },
            options: JsonOptions);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Razorpay order creation failed ({(int)response.StatusCode}): {body}");
        }

        var order = JsonSerializer.Deserialize<RazorpayOrderResponse>(body, JsonOptions);
        if (string.IsNullOrWhiteSpace(order?.Id))
        {
            throw new InvalidOperationException("Razorpay order creation returned no order id.");
        }

        return order.Id;
    }

    public async Task<string?> GetPaymentStatusAsync(string razorpayPaymentId, CancellationToken cancellationToken)
    {
        var razorpay = options.Value;
        if (!razorpay.IsConfigured)
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"payments/{razorpayPaymentId}");
        request.Headers.Authorization = CreateBasicAuth(razorpay.KeyId, razorpay.KeySecret);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var payment = JsonSerializer.Deserialize<RazorpayPaymentResponse>(body, JsonOptions);
        return payment?.Status;
    }

    public async Task<string> CreateRefundAsync(
        string razorpayPaymentId,
        long amountMinorUnits,
        CancellationToken cancellationToken)
    {
        var razorpay = options.Value;
        if (!razorpay.IsConfigured)
        {
            throw new InvalidOperationException("Razorpay is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"payments/{razorpayPaymentId}/refund");
        request.Headers.Authorization = CreateBasicAuth(razorpay.KeyId, razorpay.KeySecret);
        request.Content = JsonContent.Create(new { amount = amountMinorUnits }, options: JsonOptions);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Razorpay refund failed ({(int)response.StatusCode}): {body}");
        }

        var refund = JsonSerializer.Deserialize<RazorpayRefundResponse>(body, JsonOptions);
        return refund?.Id ?? $"refund-{razorpayPaymentId}";
    }

    private static AuthenticationHeaderValue CreateBasicAuth(string keyId, string keySecret)
    {
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        return new AuthenticationHeaderValue("Basic", token);
    }

    private sealed class RazorpayOrderResponse
    {
        public string? Id { get; set; }
    }

    private sealed class RazorpayPaymentResponse
    {
        public string? Status { get; set; }
    }

    public async Task<string> EnsurePlanAsync(
        string name,
        long amountMinorUnits,
        string period,
        int periodInterval,
        CancellationToken cancellationToken)
    {
        var razorpay = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "plans");
        request.Headers.Authorization = CreateBasicAuth(razorpay.KeyId, razorpay.KeySecret);
        request.Content = JsonContent.Create(
            new
            {
                period,
                interval = periodInterval,
                item = new { name, amount = amountMinorUnits, currency = "INR" },
            },
            options: JsonOptions);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Razorpay plan failed: {body}");
        }

        var plan = JsonSerializer.Deserialize<RazorpayPlanResponse>(body, JsonOptions);
        return plan?.Id ?? throw new InvalidOperationException("No plan id");
    }

    public async Task<string> CreateSubscriptionAsync(string planId, int totalCount, CancellationToken cancellationToken)
    {
        var razorpay = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "subscriptions");
        request.Headers.Authorization = CreateBasicAuth(razorpay.KeyId, razorpay.KeySecret);
        request.Content = JsonContent.Create(
            new
            {
                plan_id = planId,
                total_count = totalCount,
                customer_notify = 1,
            },
            options: JsonOptions);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Razorpay subscription failed: {body}");
        }

        var sub = JsonSerializer.Deserialize<RazorpaySubscriptionResponse>(body, JsonOptions);
        return sub?.Id ?? throw new InvalidOperationException("No subscription id");
    }

    public async Task<int> CountCapturedPaymentsSinceAsync(DateTimeOffset since, CancellationToken cancellationToken)
    {
        var razorpay = options.Value;
        if (!razorpay.IsConfigured)
        {
            return 0;
        }

        var from = since.ToUnixTimeSeconds();
        var to = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"payments?from={from}&to={to}&count=100");
        request.Headers.Authorization = CreateBasicAuth(razorpay.KeyId, razorpay.KeySecret);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return 0;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("items", out var items))
        {
            return 0;
        }

        return items.EnumerateArray().Count(i =>
            i.TryGetProperty("status", out var s) && s.GetString() == "captured");
    }

    private sealed class RazorpayPlanResponse
    {
        public string? Id { get; set; }
    }

    private sealed class RazorpaySubscriptionResponse
    {
        public string? Id { get; set; }
    }

    private sealed class RazorpayRefundResponse
    {
        public string? Id { get; set; }
    }
}
