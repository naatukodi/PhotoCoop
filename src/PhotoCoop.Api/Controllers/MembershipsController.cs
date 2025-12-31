using Microsoft.AspNetCore.Mvc;
using PhotoCoop.Application.Memberships;
using PhotoCoop.Application.Payments;

namespace PhotoCoop.Api.Controllers;

[ApiController]
[Route("api/memberships")]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipService _membershipService;
    private readonly IPaymentService _paymentService;

    public MembershipsController(IMembershipService membershipService, IPaymentService paymentService)
    {
        _membershipService = membershipService;
        _paymentService = paymentService;
    }

    // POST /api/memberships/renew/order
    [HttpPost("renew/order")]
    public async Task<IActionResult> StartRenewalOrder([FromBody] StartMembershipRenewalRequest request, CancellationToken cancellationToken)
    {
        var response = await _paymentService.StartMembershipRenewalAsync(request, cancellationToken);
        return Ok(response);
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
