using PhotoCoop.Domain.Users;

namespace PhotoCoop.Application.Memberships;

public interface IMembershipService
{
    Task<User> RenewMembershipAsync(RenewMembershipRequest request, CancellationToken cancellationToken = default);
    Task<User> MarkMembershipExpiredAsync(MarkMembershipExpiredRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentHistoryItemDto>> GetPaymentHistoryAsync(string photographerUserId, CancellationToken cancellationToken = default);
}

public class MembershipService : IMembershipService
{
    private readonly IUserRepository _userRepository;

    public MembershipService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
