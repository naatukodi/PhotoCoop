using PhotoCoop.Domain.Payments;
using PhotoCoop.Domain.Users;
using PhotoCoop.Application.Memberships;

namespace PhotoCoop.Application.Payments;

public interface IPaymentService
{
    Task<StartMembershipRenewalResponse> StartMembershipRenewalAsync(StartMembershipRenewalRequest request, CancellationToken ct = default);
    Task<bool> HandleWebhookAsync(RazorpayWebhookEvent evt, CancellationToken ct = default);
    Task RefundAsync(string paymentAttemptId, string reason, CancellationToken ct = default);
}

public class PaymentService : IPaymentService
{
    private readonly IUserRepository _userRepo;
    private readonly IPaymentAttemptRepository _attemptRepo;
    private readonly IRazorpayClient _razorpay;
    private readonly RazorpayOptions _rzpOptions;
    private readonly IMembershipService _membershipService;

    public PaymentService(
        IUserRepository userRepo,
        IPaymentAttemptRepository attemptRepo,
        IRazorpayClient razorpay,
        Microsoft.Extensions.Options.IOptions<RazorpayOptions> rzpOptions,
        IMembershipService membershipService)
    {
        _userRepo = userRepo;
        _attemptRepo = attemptRepo;
        _razorpay = razorpay;
        _rzpOptions = rzpOptions.Value;
        _membershipService = membershipService;
    }

    public async Task<StartMembershipRenewalResponse> StartMembershipRenewalAsync(StartMembershipRenewalRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(request.PhotographerUserId, ct);
        if (user == null || user.UserType != UserType.Photographer)
            throw new InvalidOperationException("Invalid photographer.");

        // Razorpay wants smallest unit: INR -> paise
        var amountPaise = (int)Math.Round(request.Fee * 100m, MidpointRounding.AwayFromZero);

        var receipt = $"memrenew_{request.PhotographerUserId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var order = await _razorpay.CreateOrderAsync(new RazorpayOrderCreateRequest
        {
            AmountPaise = amountPaise,
            Currency = request.Currency,
            Receipt = receipt,
            Notes = new Dictionary<string, string>
            {
                ["purpose"] = "MembershipRenewal",
                ["photographerUserId"] = request.PhotographerUserId
            }
        }, ct);

        var attempt = new PaymentAttempt(
            request.PhotographerUserId,
            request.Fee,
            request.Currency,
            razorpayOrderId: order.Id,
            renewalDateUtc: request.RenewalDateUtc,
            changedByAdminUserId: request.ChangedByAdminUserId);

        attempt = await _attemptRepo.AddAsync(attempt, ct);

        return new StartMembershipRenewalResponse
        {
            PaymentAttemptId = attempt.Id,
            RazorpayOrderId = order.Id,
            RazorpayKeyId = _rzpOptions.KeyId,
            AmountInSmallestUnit = amountPaise,
            Currency = request.Currency
        };
    }

    public async Task<bool> HandleWebhookAsync(RazorpayWebhookEvent evt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(evt.OrderId))
            return false;

        if (evt.Event == "payment.captured")
        {
            // locate attempt by order_id
            var attempt = await _attemptRepo.GetByRazorpayOrderIdAsync(evt.OrderId!, ct);
            if (attempt == null) return false; // ignore or log

            // idempotency: if already paid/refunded etc, ignore
            if (attempt.Status == PaymentAttemptStatus.Paid || attempt.Status == PaymentAttemptStatus.Refunded)
                return true;

            attempt.MarkPaid(evt.PaymentId!, evt.SignatureMaybe);
            await _attemptRepo.UpdateAsync(attempt, ct);

            // ✅ Renew membership ONLY on webhook
            await _membershipService.RenewMembershipFromPaymentAttemptAsync(attempt.Id, ct);
            return true;
        }
        else if (evt.Event == "payment.failed")
        {
            var attempt = await _attemptRepo.GetByRazorpayOrderIdAsync(evt.OrderId!, ct);
            if (attempt == null) return false;

            if (attempt.Status == PaymentAttemptStatus.Paid || attempt.Status == PaymentAttemptStatus.Refunded)
                return true;

            attempt.MarkFailed();
            await _attemptRepo.UpdateAsync(attempt, ct);
            return true;
        }

        return false;
    }

    public async Task RefundAsync(string paymentAttemptId, string reason, CancellationToken ct = default)
    {
        var attempt = await _attemptRepo.GetByIdAsync(paymentAttemptId, ct);
        if (attempt == null) throw new KeyNotFoundException("Payment attempt not found.");

        if (attempt.Status == PaymentAttemptStatus.Refunded)
            return;

        if (string.IsNullOrWhiteSpace(attempt.RazorpayPaymentId))
            throw new InvalidOperationException("No captured payment id to refund.");

        var refund = await _razorpay.RefundPaymentAsync(
            attempt.RazorpayPaymentId,
            new RazorpayRefundRequest
            {
                Notes = string.IsNullOrWhiteSpace(reason)
                    ? null
                    : new Dictionary<string, string> { ["reason"] = reason }
            },
            ct);

        attempt.MarkRefunded(refund.Id);
        await _attemptRepo.UpdateAsync(attempt, ct);
    }
}
