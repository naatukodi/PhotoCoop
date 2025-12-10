using PhotoCoop.Domain.Users;

namespace PhotoCoop.Application.Bookings;

public class CreateBookingRequest
{
    public string CustomerUserId { get; set; } = null!;
    public OccasionType Occasion { get; set; }
    public DateTime EventDate { get; set; }
    public string EventLine1 { get; set; } = null!;
    public string EventCity { get; set; } = null!;
    public string EventState { get; set; } = null!;
    public string EventPincode { get; set; } = null!;
    public int? ExpectedHours { get; set; }
    public int? ExpectedPhotographersCount { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public string? SpecialRequirements { get; set; }
}

public class AssignPhotographerRequest
{
    public string BookingId { get; set; } = null!;
    public string PhotographerUserId { get; set; } = null!;
    public string AdminUserId { get; set; } = null!;
    public decimal? FinalPrice { get; set; }
}
