using Microsoft.AspNetCore.Mvc;
using PhotoCoop.Application.Matching;

namespace PhotoCoop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchingController : ControllerBase
{
    private readonly IPhotographerMatchingService _matchingService;

    public MatchingController(IPhotographerMatchingService matchingService)
    {
        _matchingService = matchingService;
    }

    [HttpGet("bookings/{bookingId}/photographers")]
    public async Task<IActionResult> GetMatchesForBooking(string bookingId, CancellationToken cancellationToken)
    {
        var matches = await _matchingService.FindMatchesForBookingAsync(bookingId, cancellationToken);
        return Ok(matches);
    }
}
