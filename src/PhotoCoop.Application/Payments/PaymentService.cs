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

        var receipt = BuildRazorpayReceipt("memren", request.PhotographerUserId);
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
        Console.WriteLine($"[PaymentWebhook] Received event={evt.Event}, orderId={evt.OrderId}, paymentId={evt.PaymentId}, signature={evt.SignatureMaybe}");

        if (string.IsNullOrWhiteSpace(evt.OrderId))
        {
            Console.WriteLine("[PaymentWebhook] Missing order id, ignoring webhook.");
            return false;
        }

        if (evt.Event is "payment.captured" or "payment.authorized")
        {
            // locate attempt by order_id
            var attempt = await _attemptRepo.GetByRazorpayOrderIdAsync(evt.OrderId!, ct);
            if (attempt == null)
            {
                Console.WriteLine($"[PaymentWebhook] No payment attempt found for order {evt.OrderId}.");
                return false; // ignore or log
            }

            // idempotency: if already paid/refunded etc, ignore
            if (attempt.Status == PaymentAttemptStatus.Paid || attempt.Status == PaymentAttemptStatus.Refunded)
            {
                Console.WriteLine($"[PaymentWebhook] Attempt {attempt.Id} already settled with status {attempt.Status}, skipping.");
                return true;
            }

            if (string.IsNullOrWhiteSpace(evt.PaymentId))
            {
                Console.WriteLine($"[PaymentWebhook] Missing payment id for event {evt.Event} on order {evt.OrderId}, ignoring.");
                return false;
            }

            attempt.MarkPaid(evt.PaymentId!, evt.SignatureMaybe);
            await _attemptRepo.UpdateAsync(attempt, ct);

            // ✅ Renew membership ONLY on webhook
            Console.WriteLine($"[PaymentWebhook] Attempt {attempt.Id} marked paid via {evt.Event}. Renewing membership.");
            await _membershipService.RenewMembershipFromPaymentAttemptAsync(attempt, ct);
            Console.WriteLine($"[PaymentWebhook] Membership renewal complete for attempt {attempt.Id}.");
            return true;
        }
        else if (evt.Event == "payment.failed")
        {
            var attempt = await _attemptRepo.GetByRazorpayOrderIdAsync(evt.OrderId!, ct);
            if (attempt == null)
            {
                Console.WriteLine($"[PaymentWebhook] No payment attempt found for failed order {evt.OrderId}.");
                return false;
            }

            if (attempt.Status == PaymentAttemptStatus.Paid || attempt.Status == PaymentAttemptStatus.Refunded)
            {
                Console.WriteLine($"[PaymentWebhook] Attempt {attempt.Id} already settled with status {attempt.Status}, skipping fail handling.");
                return true;
            }

            attempt.MarkFailed();
            await _attemptRepo.UpdateAsync(attempt, ct);
            Console.WriteLine($"[PaymentWebhook] Attempt {attempt.Id} marked failed.");
            return true;
        }

        Console.WriteLine($"[PaymentWebhook] Unhandled event type {evt.Event}.");
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

    private static string BuildRazorpayReceipt(string prefix, string id)
    {
        const int max = 40;
        var ts = DateTime.UtcNow.ToString("yyMMddHHmmss");
        var unique = Guid.NewGuid().ToString("N")[..6];
        var suffix = $"_{ts}_{unique}"; // 1 + 12 + 1 + 6 = 20 chars

        // leave room for prefix + '_' + suffix
        var remaining = max - prefix.Length - suffix.Length - 1;
        if (remaining < 0) remaining = 0;

        var trimmedId = id.Length > remaining ? id[..remaining] : id;
        return $"{prefix}_{trimmedId}{suffix}";
    }
}
