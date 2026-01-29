using Microsoft.AspNetCore.Mvc;
using PhotoCoop.Application.Fundraising;

[ApiController]
[Route("api/fundraising")]
public class FundraisingController : ControllerBase
{
    private readonly IFundraisingService _svc;
    public FundraisingController(IFundraisingService svc) => _svc = svc;

    // Admin: create a campaign
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateFundraisingEventRequest req, CancellationToken ct)
    {
        var ev = await _svc.CreateEventAsync(req, ct);
        return Ok(ev);
    }

    // Admin: activate a campaign
    [HttpPost("{eventId}/activate")]
    public async Task<IActionResult> ActivateEvent(string eventId, CancellationToken ct)
    {
        var resp = await _svc.ActivateEventAsync(new ActivateFundraisingEventRequest { EventId = eventId }, ct);
        return Ok(resp);
    }

    [HttpPost("{eventId}/donations/start")]
    public async Task<IActionResult> StartDonation(string eventId, [FromBody] StartDonationRequest req, CancellationToken ct)
    {
        req.EventId = eventId;
        var resp = await _svc.StartDonationAsync(req, ct);
        return Ok(resp);
    }
}
