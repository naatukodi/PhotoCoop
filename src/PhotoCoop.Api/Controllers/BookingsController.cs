using Microsoft.AspNetCore.Mvc;
using PhotoCoop.Application.Bookings;

namespace PhotoCoop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CreateBookingAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingById(string id, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.GetBookingAsync(id, cancellationToken);
        if (booking == null) return NotFound();
        return Ok(booking);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignPhotographer([FromBody] AssignPhotographerRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.AssignPhotographerAsync(request, cancellationToken);
        return Ok(booking);
    }

    [HttpGet("customers/{customerUserId}")]
    public async Task<IActionResult> GetByCustomer(string customerUserId, CancellationToken cancellationToken)
    {
        var list = await _bookingService.GetBookingsByCustomerAsync(customerUserId, cancellationToken);
        return Ok(list);
    }

    [HttpGet("photographers/{photographerUserId}")]
    public async Task<IActionResult> GetByPhotographer(string photographerUserId, CancellationToken cancellationToken)
    {
        var list = await _bookingService.GetBookingsByPhotographerAsync(photographerUserId, cancellationToken);
        return Ok(list);
    }

    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateBookingStatusRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.UpdateStatusAsync(request, cancellationToken);
        return Ok(booking);
    }
}
