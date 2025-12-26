namespace PhotoCoop.Domain.Payments;

using System.Text.Json.Serialization;

public class RazorpayOrderCreateRequest
{
    [JsonPropertyName("amount")]
    public int AmountPaise { get; set; }          // Razorpay uses paise

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "INR";

    [JsonPropertyName("receipt")]
    public string Receipt { get; set; } = null!;

    [JsonPropertyName("notes")]
    public Dictionary<string, string>? Notes { get; set; }
}

public class RazorpayOrderCreateResponse
{
    public string Id { get; set; } = null!;       // order_id
    public int Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Receipt { get; set; } = null!;
}

public class RazorpayRefundRequest
{
    [JsonPropertyName("amount")]
    public int? AmountPaise { get; set; } // optional partial refund

    [JsonPropertyName("notes")]
    public Dictionary<string, string>? Notes { get; set; }
}

public class RazorpayRefundResponse
{
    public string Id { get; set; } = null!;
    public int Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public interface IRazorpayClient
{
    Task<RazorpayOrderCreateResponse> CreateOrderAsync(RazorpayOrderCreateRequest req, CancellationToken ct = default);
    Task<RazorpayRefundResponse> RefundPaymentAsync(string razorpayPaymentId, RazorpayRefundRequest req, CancellationToken ct = default);
}
