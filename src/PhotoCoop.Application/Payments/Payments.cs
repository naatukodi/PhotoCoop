namespace PhotoCoop.Application.Payments;

public class StartMembershipRenewalRequest
{
    public string PhotographerUserId { get; set; } = null!;
    public decimal Fee { get; set; }           // in INR
    public string Currency { get; set; } = "INR";
    public DateTime RenewalDateUtc { get; set; }
    public string? ChangedByAdminUserId { get; set; }
}

public class StartMembershipRenewalResponse
{
    public string PaymentAttemptId { get; set; } = null!;
    public string RazorpayOrderId { get; set; } = null!;
    public string RazorpayKeyId { get; set; } = null!;
    public int AmountInSmallestUnit { get; set; }          // paise
    public string Currency { get; set; } = "INR";
}
