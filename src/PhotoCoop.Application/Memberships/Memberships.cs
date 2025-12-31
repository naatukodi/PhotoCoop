using PhotoCoop.Domain.Users;

namespace PhotoCoop.Application.Memberships;

public class RenewMembershipRequest
{
    public string PhotographerUserId { get; set; } = null!;

    public DateTime RenewalDateUtc { get; set; }
    public decimal Fee { get; set; }
    public string Currency { get; set; } = "INR";

    public PaymentMode Mode { get; set; }
    public PaymentStatus Status { get; set; }
    public string? GatewayTransactionId { get; set; }

    // Audit
    public string? ChangedByAdminUserId { get; set; }
}

public class MarkMembershipExpiredRequest
{
    public string PhotographerUserId { get; set; } = null!;
    public string? ChangedByAdminUserId { get; set; }
    public string? Remarks { get; set; }
}

public class PaymentHistoryItemDto
{
    public string PaymentId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public PaymentMode Mode { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaidAtUtc { get; set; }
    public string? GatewayTransactionId { get; set; }
}
