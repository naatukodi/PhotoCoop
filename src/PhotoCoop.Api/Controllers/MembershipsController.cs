using Microsoft.AspNetCore.Mvc;
using PhotoCoop.Application.Memberships;

namespace PhotoCoop.Api.Controllers;

[ApiController]
[Route("api/memberships")]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipsController(IMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    // POST /api/memberships/renew
    [HttpPost("renew")]
    public async Task<IActionResult> Renew([FromBody] RenewMembershipRequest request, CancellationToken cancellationToken)
    {
        var updatedUser = await _membershipService.RenewMembershipAsync(request, cancellationToken);
        return Ok(updatedUser);
    }

    // POST /api/memberships/expire
    [HttpPost("expire")]
    public async Task<IActionResult> Expire([FromBody] MarkMembershipExpiredRequest request, CancellationToken cancellationToken)
    {
        var updatedUser = await _membershipService.MarkMembershipExpiredAsync(request, cancellationToken);
        return Ok(updatedUser);
    }

    // GET /api/memberships/{photographerUserId}/payments
    [HttpGet("{photographerUserId}/payments")]
    public async Task<IActionResult> PaymentHistory(string photographerUserId, CancellationToken cancellationToken)
    {
        var history = await _membershipService.GetPaymentHistoryAsync(photographerUserId, cancellationToken);
        return Ok(history);
    }
}
