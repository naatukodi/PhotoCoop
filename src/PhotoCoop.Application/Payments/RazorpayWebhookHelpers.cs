using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PhotoCoop.Application.Payments;

public record RazorpayWebhookEvent(string Event, string? OrderId, string? PaymentId, string? SignatureMaybe);

public static class RazorpaySignatureVerifier
{
    public static bool VerifyWebhook(string rawBody, string receivedSignature, string webhookSecret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
        var bodyBytes = Encoding.UTF8.GetBytes(rawBody);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(bodyBytes);
        var expected = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        return SlowEquals(expected, receivedSignature);
    }

    // constant-time compare
    private static bool SlowEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
}

public static class RazorpayWebhookParser
{
    public static RazorpayWebhookEvent Parse(string rawBody)
    {
        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;

        var eventName = root.TryGetProperty("event", out var evtProp)
            ? evtProp.GetString() ?? string.Empty
            : string.Empty;

        string? paymentId = null;
        string? orderId = null;
        string? signature = null;

        if (root.TryGetProperty("payload", out var payload))
        {
            if (payload.TryGetProperty("payment", out var payment) &&
                payment.TryGetProperty("entity", out var paymentEntity))
            {
                if (paymentEntity.TryGetProperty("id", out var pidProp))
                    paymentId = pidProp.GetString();

                if (paymentEntity.TryGetProperty("order_id", out var orderIdProp))
                    orderId = orderIdProp.GetString();

                if (paymentEntity.TryGetProperty("signature", out var sigProp))
                    signature = sigProp.GetString();
            }

            if (string.IsNullOrEmpty(orderId) &&
                payload.TryGetProperty("order", out var order) &&
                order.TryGetProperty("entity", out var orderEntity) &&
                orderEntity.TryGetProperty("id", out var oidProp))
            {
                orderId = oidProp.GetString();
            }
        }

        return new RazorpayWebhookEvent(eventName, orderId, paymentId, signature);
    }
}
