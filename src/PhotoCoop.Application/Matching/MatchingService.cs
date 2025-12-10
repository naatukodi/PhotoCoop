using PhotoCoop.Domain.Bookings;
using PhotoCoop.Domain.Users;

namespace PhotoCoop.Application.Matching;

public class PhotographerMatchResult
{
    public string PhotographerUserId { get; set; } = null!;
    public string PhotographerName { get; set; } = null!;
    public decimal? EstimatedPrice { get; set; }
    public int Score { get; set; }  // higher = better match
}

public interface IPhotographerMatchingService
{
    Task<IReadOnlyList<PhotographerMatchResult>> FindMatchesForBookingAsync(string bookingId, CancellationToken cancellationToken = default);
}

public class PhotographerMatchingService : IPhotographerMatchingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;

    public PhotographerMatchingService(IBookingRepository bookingRepository, IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<PhotographerMatchResult>> FindMatchesForBookingAsync(
        string bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException("Booking not found.");

        var eventPincode = booking.EventAddress.Pincode;
        var occasion = booking.Occasion;

        var photographers = await _userRepository.GetPhotographersByPincodeAndOccasionAsync(
            eventPincode, occasion, cancellationToken);

        var results = new List<PhotographerMatchResult>();

        foreach (var p in photographers)
        {
            if (p.PhotographerProfile == null)
                continue;

            // Simple heuristic for now: take first rate card for that occasion
            var rateCard = p.PhotographerProfile.RateCards
                .FirstOrDefault(rc => rc.Occasion == occasion);

            decimal? price = rateCard?.BasePrice;

            // Optionally adjust price using ExpectedHours / Budget
            var score = 0;

            if (booking.BudgetMin.HasValue && booking.BudgetMax.HasValue && price.HasValue)
            {
                if (price.Value >= booking.BudgetMin.Value && price.Value <= booking.BudgetMax.Value)
                    score += 30;
                else
                    score += 10; // partial match
            }

            // add score weight for rating
            if (p.PhotographerProfile.AverageRating.HasValue)
            {
                score += (int)(p.PhotographerProfile.AverageRating.Value * 5); // 0-25 points
            }

            // add score weight for experience
            score += Math.Min(p.PhotographerProfile.YearsOfExperience * 2, 20); // cap at 20

            results.Add(new PhotographerMatchResult
            {
                PhotographerUserId = p.Id,
                PhotographerName = p.PhotographerProfile.DisplayName,
                EstimatedPrice = price,
                Score = score
            });
        }

        return results
            .OrderByDescending(r => r.Score)
            .Take(20) // cap for UI
            .ToList();
    }
}
