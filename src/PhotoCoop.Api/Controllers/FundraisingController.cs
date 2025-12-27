using Microsoft.AspNetCore.Mvc;
using PhotoCoop.Application.Fundraising;

[ApiController]
[Route("api/fundraising")]
public class FundraisingController : ControllerBase
{
    private readonly IFundraisingService _svc;
    public FundraisingController(IFundraisingService svc) => _svc = svc;

    [HttpPost("{eventId}/donations/start")]
    public async Task<IActionResult> StartDonation(string eventId, [FromBody] StartDonationRequest req, CancellationToken ct)
    {
        req.EventId = eventId;
        var resp = await _svc.StartDonationAsync(req, ct);
        return Ok(resp);
    }
}
