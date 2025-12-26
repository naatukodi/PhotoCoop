namespace PhotoCoop.Domain.Payments;

public class RazorpayOptions
{
    public const string SectionName = "Razorpay";

    public string KeyId { get; set; } = null!;
    public string KeySecret { get; set; } = null!;
    public string WebhookSecret { get; set; } = null!;
}
