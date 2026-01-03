using PhotoCoop.Domain.Users;
using PhotoCoop.Domain.Payments;

namespace PhotoCoop.Application.Memberships;

public interface IMembershipService
{
    Task<User> RenewMembershipAsync(RenewMembershipRequest request, CancellationToken cancellationToken = default);
    Task<User> MarkMembershipExpiredAsync(MarkMembershipExpiredRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentHistoryItemDto>> GetPaymentHistoryAsync(string photographerUserId, CancellationToken cancellationToken = default);
    Task<User> RenewMembershipFromPaymentAttemptAsync(PaymentAttempt paymentAttempt, CancellationToken cancellationToken = default);

}

public class MembershipService : IMembershipService
{
    private readonly IUserRepository _userRepository;
    private readonly IPaymentAttemptRepository _attemptRepository;

    public MembershipService(IUserRepository userRepository, IPaymentAttemptRepository attemptRepository)
    {
        _userRepository = userRepository;
        _attemptRepository = attemptRepository;
    }

    public async Task<User> RenewMembershipAsync(RenewMembershipRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.PhotographerUserId, cancellationToken);
        if (user == null || user.UserType != UserType.Photographer || user.PhotographerProfile == null)
            throw new InvalidOperationException("Invalid photographer.");

        // Optional: enforce that only mapped admin can renew
        // if (user.PhotographerProfile.MappedAdminUserId != request.ChangedByAdminUserId) { ... }

        var payment = new PaymentDetails(
            request.Fee,
            request.Mode,
            request.Status,
            request.Currency,
            request.GatewayTransactionId
        );

        user.PhotographerProfile.RenewMembership(request.RenewalDateUtc, request.Fee, payment);

        return await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<User> RenewMembershipFromPaymentAttemptAsync(PaymentAttempt paymentAttempt, CancellationToken cancellationToken = default)
    {
        if (paymentAttempt == null) throw new ArgumentNullException(nameof(paymentAttempt));

        // The webhook already marked the attempt paid; avoid a stale read by trusting the passed attempt.
        if (paymentAttempt.Status != PaymentAttemptStatus.Paid)
            throw new InvalidOperationException("Payment is not successful.");

        var user = await _userRepository.GetByIdAsync(paymentAttempt.PhotographerUserId, cancellationToken);
        if (user == null || user.UserType != UserType.Photographer || user.PhotographerProfile == null)
            throw new InvalidOperationException("Invalid photographer.");

        // Idempotency: if renewal already moved past target date, do nothing
        var currentRenewal = user.PhotographerProfile.Membership.RenewalDateUtc;
        if (currentRenewal.HasValue && currentRenewal.Value >= paymentAttempt.RenewalDateUtc)
            return user;

        // Record payment in membership history (map Razorpay -> domain payment model)
        var payment = new PaymentDetails(
            amount: paymentAttempt.Amount,
            mode: PhotoCoop.Domain.Users.PaymentMode.Other,     // Razorpay -> map based on webhook payload later
            status: PhotoCoop.Domain.Users.PaymentStatus.Paid,
            currency: paymentAttempt.Currency,
            gatewayTransactionId: paymentAttempt.RazorpayPaymentId
        );

        user.PhotographerProfile.RenewMembership(paymentAttempt.RenewalDateUtc, paymentAttempt.Amount, payment);

        return await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<User> MarkMembershipExpiredAsync(MarkMembershipExpiredRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.PhotographerUserId, cancellationToken);
        if (user == null || user.UserType != UserType.Photographer || user.PhotographerProfile == null)
            throw new InvalidOperationException("Invalid photographer.");

        user.PhotographerProfile.MarkMembershipExpired();

        return await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentHistoryItemDto>> GetPaymentHistoryAsync(string photographerUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(photographerUserId, cancellationToken);
        if (user == null || user.UserType != UserType.Photographer || user.PhotographerProfile == null)
            throw new InvalidOperationException("Invalid photographer.");

        var history = user.PhotographerProfile.Membership.PaymentHistory
            .OrderByDescending(p => p.PaidAtUtc)
            .Select(p => new PaymentHistoryItemDto
            {
                PaymentId = p.PaymentId,
                Amount = p.Amount,
                Currency = p.Currency,
                Mode = p.Mode,
                Status = p.Status,
                PaidAtUtc = p.PaidAtUtc,
                GatewayTransactionId = p.GatewayTransactionId
            })
            .ToList();

        return history;
    }
}
