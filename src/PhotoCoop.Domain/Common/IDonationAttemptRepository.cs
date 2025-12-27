namespace PhotoCoop.Domain.Fundraising;

public interface IDonationAttemptRepository : PhotoCoop.Domain.Common.IRepository<DonationAttempt>
{
    Task<DonationAttempt?> GetByRazorpayOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
    Task<DonationAttempt?> GetByReceiptAsync(string receipt, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DonationAttempt>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
}
