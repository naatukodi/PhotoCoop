namespace PhotoCoop.Application.Fundraising;

public class StartDonationRequest
{
    public string EventId { get; set; } = null!;
    public long AmountMinor { get; set; } // paise
    public string Currency { get; set; } = "INR";

    // donor
    public string? DonorUserId { get; set; }
    public string? DonorName { get; set; }
    public string? DonorEmail { get; set; }
    public string? DonorPhone { get; set; }
    public bool IsAnonymous { get; set; } = false;
}

public class StartDonationResponse
{
    public string RazorpayKeyId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public long AmountMinor { get; set; }
    public string Currency { get; set; } = null!;
    public string Receipt { get; set; } = null!;
    public string EventId { get; set; } = null!;
}
