using System.Security.Cryptography;
using System.Text;

namespace MoviePlatform.Infrastructure.Commerce;

public static class RazorpaySignature
{
    public static bool VerifyPayment(string orderId, string paymentId, string signature, string keySecret)
    {
        if (string.IsNullOrWhiteSpace(keySecret) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var payload = $"{orderId}|{paymentId}";
        var expected = ComputeHmacSha256(payload, keySecret);
        return FixedTimeEquals(expected, signature);
    }

    public static bool VerifySubscriptionPayment(
        string subscriptionId,
        string paymentId,
        string signature,
        string keySecret)
    {
        if (string.IsNullOrWhiteSpace(keySecret) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var payload = $"{paymentId}|{subscriptionId}";
        var expected = ComputeHmacSha256(payload, keySecret);
        return FixedTimeEquals(expected, signature);
    }

    public static bool VerifyWebhook(string rawBody, string signature, string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(webhookSecret) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var expected = ComputeHmacSha256(rawBody, webhookSecret);
        return FixedTimeEquals(expected, signature);
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}
