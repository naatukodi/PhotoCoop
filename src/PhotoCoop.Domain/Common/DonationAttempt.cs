namespace PhotoCoop.Domain.Fundraising;

public enum PaymentAttemptType
{
    MembershipRenewal = 1,
    Donation = 2
}

public enum DonationAttemptStatus
{
    Created = 1,           // order created
    CheckoutOpened = 2,    // optional (if you track)
    PaymentAuthorized = 3, // if you handle
    Paid = 4,              // captured/paid
    Failed = 5,
    Refunded = 6
}

public class DonationAttempt : PhotoCoop.Domain.Common.Entity
{
    // Partition suggestion: /eventId or /id (see notes)
    public string EventId { get; private set; } = null!;

    // Donor info
    public string? DonorUserId { get; private set; }          // if logged in
    public string? DonorName { get; private set; }            // if anonymous
    public string? DonorEmail { get; private set; }
    public string? DonorPhone { get; private set; }
    public bool IsAnonymous { get; private set; } = false;

    // Amounts in minor units (paise)
    public long AmountMinor { get; private set; }
    public string Currency { get; private set; } = "INR";

    // Razorpay fields
    public string RazorpayOrderId { get; private set; } = null!;
    public string? RazorpayPaymentId { get; private set; }
    public string? RazorpaySignature { get; private set; }

    public DonationAttemptStatus Status { get; private set; } = DonationAttemptStatus.Created;

    // idempotency / bookkeeping
    public string Receipt { get; private set; } = null!; // your unique receipt
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    // Optional: refund details
    public string? RazorpayRefundId { get; private set; }
    public string? FailureReason { get; private set; }

    private DonationAttempt() { }

    public DonationAttempt(
        string eventId,
        long amountMinor,
        string currency,
        string razorpayOrderId,
        string receipt,
        string? donorUserId,
        string? donorName,
        string? donorEmail,
        string? donorPhone,
        bool isAnonymous)
    {
        EventId = eventId;
        AmountMinor = amountMinor;
        Currency = currency;
        RazorpayOrderId = razorpayOrderId;
        Receipt = receipt;

        DonorUserId = donorUserId;
        DonorName = donorName;
        DonorEmail = donorEmail;
        DonorPhone = donorPhone;
        IsAnonymous = isAnonymous;

        Status = DonationAttemptStatus.Created;
    }

    public void MarkPaid(string razorpayPaymentId, string razorpaySignature)
    {
        RazorpayPaymentId = razorpayPaymentId;
        RazorpaySignature = razorpaySignature;
        Status = DonationAttemptStatus.Paid;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = DonationAttemptStatus.Failed;
        FailureReason = reason;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkRefunded(string refundId)
    {
        Status = DonationAttemptStatus.Refunded;
        RazorpayRefundId = refundId;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
