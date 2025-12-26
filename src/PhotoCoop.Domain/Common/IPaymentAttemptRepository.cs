using PhotoCoop.Domain.Common;

namespace PhotoCoop.Domain.Payments;

public interface IPaymentAttemptRepository
{
    Task<PaymentAttempt?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<PaymentAttempt?> GetByRazorpayOrderIdAsync(string razorpayOrderId, CancellationToken ct = default);
    Task<PaymentAttempt?> GetByRazorpayPaymentIdAsync(string razorpayPaymentId, CancellationToken ct = default);

    Task<PaymentAttempt> AddAsync(PaymentAttempt attempt, CancellationToken ct = default);
    Task<PaymentAttempt> UpdateAsync(PaymentAttempt attempt, CancellationToken ct = default);

    Task<IReadOnlyList<PaymentAttempt>> GetByPhotographerAsync(string photographerUserId, int take = 50, CancellationToken ct = default);
}
