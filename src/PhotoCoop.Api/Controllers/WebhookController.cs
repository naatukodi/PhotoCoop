using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PhotoCoop.Application.Payments;
using PhotoCoop.Application.Fundraising;
using PhotoCoop.Domain.Payments;

namespace PhotoCoop.Api.Controllers;

[ApiController]
[Route("api/webhooks/razorpay")]
public class WebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IDonationWebhookService _donationService;
    private readonly RazorpayOptions _rzpOptions;

    public WebhookController(
        IPaymentService paymentService,
        IDonationWebhookService donationService,
        IOptions<RazorpayOptions> rzpOptions)
    {
        _paymentService = paymentService;
        _donationService = donationService;
        _rzpOptions = rzpOptions.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);

        var signature = Request.Headers["X-Razorpay-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signature))
            return Unauthorized();

        if (!RazorpaySignatureVerifier.VerifyWebhook(rawBody, signature, _rzpOptions.WebhookSecret))
            return Unauthorized();

        var evt = RazorpayWebhookParser.Parse(rawBody);

        var membershipHandled = await _paymentService.HandleWebhookAsync(evt, ct);
        var donationHandled = membershipHandled
            ? false
            : await _donationService.HandleWebhookAsync(evt, ct);

        return Ok(new { membershipHandled, donationHandled });
    }
}
