using PhotoCoop.Domain.Fundraising;
using PhotoCoop.Application.Payments;

namespace PhotoCoop.Application.Fundraising;

public interface IDonationWebhookService
{
    Task<bool> HandleWebhookAsync(RazorpayWebhookEvent evt, CancellationToken ct);
}

public class DonationWebhookService : IDonationWebhookService
{
    private readonly IDonationAttemptRepository _attemptRepo;
    private readonly IFundraisingEventRepository _eventRepo;

    public DonationWebhookService(IDonationAttemptRepository attemptRepo, IFundraisingEventRepository eventRepo)
    {
        _attemptRepo = attemptRepo;
        _eventRepo = eventRepo;
    }

    public async Task<bool> HandleWebhookAsync(RazorpayWebhookEvent evt, CancellationToken ct)
    {
        var orderId = evt.OrderId;
        var paymentId = evt.PaymentId;
        var eventName = evt.Event;

        if (string.IsNullOrWhiteSpace(orderId))
            return false;

        var attempt = await _attemptRepo.GetByRazorpayOrderIdAsync(orderId!, ct);
        if (attempt == null)
            return false; // not donation

        // Idempotency: already paid/refunded etc.
        if (attempt.Status == DonationAttemptStatus.Paid || attempt.Status == DonationAttemptStatus.Refunded)
            return true;

        if (eventName == "payment.captured" || eventName == "payment.authorized") // choose your final
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                return true;

            attempt.MarkPaid(paymentId!, razorpaySignature: evt.SignatureMaybe ?? string.Empty);
            await _attemptRepo.UpdateAsync(attempt, ct);

            // ✅ Update fundraising totals (denormalized)
            var ev = await _eventRepo.GetByIdAsync(attempt.EventId, ct);
            if (ev != null)
            {
                ev.AddSuccessfulDonation(attempt.AmountMinor);
                await _eventRepo.UpdateAsync(ev, ct);
            }

            return true;
        }

        if (eventName == "payment.failed")
        {
            attempt.MarkFailed("payment.failed");
            await _attemptRepo.UpdateAsync(attempt, ct);
            return true;
        }

        // handle refunds too if you want: "refund.processed"
        return true;
    }
}
