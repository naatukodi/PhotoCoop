using PhotoCoop.Domain.Common;

namespace PhotoCoop.Domain.Payments;

public enum PaymentAttemptStatus
{
    Created = 1,          // order created, checkout not completed yet
    Paid = 2,             // payment authorized/captured (as per webhook)
    Failed = 3,
    Expired = 4,
    Refunded = 5
}

public class PaymentAttempt : Entity
{
    // Cosmos
    public string PartitionKey => PhotographerUserId; // recommended

    // Business
    public string PhotographerUserId { get; private set; } = null!;
    public string Purpose { get; private set; } = "MembershipRenewal";

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "INR";

    // Razorpay references
    public string RazorpayOrderId { get; private set; } = null!;
    public string? RazorpayPaymentId { get; private set; }
    public string? RazorpaySignature { get; private set; }

    public PaymentAttemptStatus Status { get; private set; } = PaymentAttemptStatus.Created;

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? PaidAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    // Membership intent
    public DateTime RenewalDateUtc { get; private set; }
    public string? ChangedByAdminUserId { get; private set; }

    // Refund info
    public string? RazorpayRefundId { get; private set; }
    public DateTime? RefundedAtUtc { get; private set; }

    private PaymentAttempt() { }

    public PaymentAttempt(
        string photographerUserId,
        decimal amount,
        string currency,
        string razorpayOrderId,
        DateTime renewalDateUtc,
        string? changedByAdminUserId)
    {
        PhotographerUserId = photographerUserId;
        Amount = amount;
        Currency = currency;
        RazorpayOrderId = razorpayOrderId;
        RenewalDateUtc = renewalDateUtc;
        ChangedByAdminUserId = changedByAdminUserId;
    }

    public void MarkPaid(string razorpayPaymentId, string? razorpaySignature)
    {
        RazorpayPaymentId = razorpayPaymentId;
        RazorpaySignature = razorpaySignature;
        Status = PaymentAttemptStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = PaymentAttemptStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkRefunded(string refundId)
    {
        RazorpayRefundId = refundId;
        Status = PaymentAttemptStatus.Refunded;
        RefundedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
