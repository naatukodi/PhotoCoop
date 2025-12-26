using Microsoft.AspNetCore.Mvc;
using PhotoCoop.Application.Payments;

namespace PhotoCoop.Api.Controllers;

[ApiController]
[Route("api/webhooks/razorpay")]
public class WebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public WebhookController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);

        var signature = Request.Headers["X-Razorpay-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signature))
            return Unauthorized();

        await _paymentService.HandleWebhookAsync(rawBody, signature, ct);
        return Ok();
    }
}
