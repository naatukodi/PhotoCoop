using PhotoCoop.Domain.Fundraising;
using PhotoCoop.Domain.Payments;

namespace PhotoCoop.Application.Fundraising;

public interface IFundraisingService
{
    Task<StartDonationResponse> StartDonationAsync(StartDonationRequest request, CancellationToken cancellationToken = default);
}

public class FundraisingService : IFundraisingService
{
    private readonly IFundraisingEventRepository _eventRepo;
    private readonly IDonationAttemptRepository _attemptRepo;
    private readonly IRazorpayClient _razorpay;
    private readonly RazorpayOptions _options;

    public FundraisingService(
        IFundraisingEventRepository eventRepo,
        IDonationAttemptRepository attemptRepo,
        IRazorpayClient razorpay,
        Microsoft.Extensions.Options.IOptions<RazorpayOptions> options)
    {
        _eventRepo = eventRepo;
        _attemptRepo = attemptRepo;
        _razorpay = razorpay;
        _options = options.Value;
    }

    public async Task<StartDonationResponse> StartDonationAsync(StartDonationRequest request, CancellationToken cancellationToken = default)
    {
        var ev = await _eventRepo.GetByIdAsync(request.EventId, cancellationToken);
        if (ev == null || ev.Status != FundraisingEventStatus.Active)
            throw new InvalidOperationException("Fundraising event is not active.");

        if (request.AmountMinor <= 0)
            throw new InvalidOperationException("Amount must be > 0.");

        // Receipt should be unique and <= 40 chars for Razorpay
        var receipt = BuildRazorpayReceipt("don", request.EventId);

        // Create Razorpay Order
        var order = await _razorpay.CreateOrderAsync(new RazorpayOrderCreateRequest
        {
            AmountPaise = checked((int)request.AmountMinor), // convert long->int for Razorpay
            Currency = request.Currency,
            Receipt = receipt,
            Notes = new Dictionary<string, string>
            {
                ["attemptType"] = "Donation",
                ["eventId"] = request.EventId
            }
        }, cancellationToken);

        // Persist attempt
        var attempt = new DonationAttempt(
            eventId: request.EventId,
            amountMinor: request.AmountMinor,
            currency: request.Currency,
            razorpayOrderId: order.Id,
            receipt: receipt,
            donorUserId: request.DonorUserId,
            donorName: request.DonorName,
            donorEmail: request.DonorEmail,
            donorPhone: request.DonorPhone,
            isAnonymous: request.IsAnonymous
        );

        await _attemptRepo.AddAsync(attempt, cancellationToken);

        return new StartDonationResponse
        {
            RazorpayKeyId = _options.KeyId, // public key used in checkout
            OrderId = order.Id,
            AmountMinor = request.AmountMinor,
            Currency = request.Currency,
            Receipt = receipt,
            EventId = request.EventId
        };
    }

    private static string BuildRazorpayReceipt(string prefix, string id)
    {
        const int max = 40;
        var ts = DateTime.UtcNow.ToString("yyMMddHHmmss");
        var unique = Guid.NewGuid().ToString("N")[..6];
        var suffix = $"_{ts}_{unique}";

        var remaining = max - prefix.Length - suffix.Length - 1;
        if (remaining < 0) remaining = 0;

        var trimmedId = id.Length > remaining ? id[..remaining] : id;
        return $"{prefix}_{trimmedId}{suffix}";
    }
}
