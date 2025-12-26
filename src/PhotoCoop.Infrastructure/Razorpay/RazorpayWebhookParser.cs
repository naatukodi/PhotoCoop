using System.Text.Json;

namespace PhotoCoop.Infrastructure.Razorpay;

public record RazorpayWebhookEvent(
    string Event,
    string? OrderId,
    string? PaymentId,
    string? SignatureMaybe);

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
