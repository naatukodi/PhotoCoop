using PhotoCoop.Domain.Bookings;
using PhotoCoop.Domain.Common;

namespace PhotoCoop.Domain.Bookings;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IReadOnlyList<Booking>> GetByCustomerAsync(
        string customerUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetByPhotographerAsync(
        string photographerUserId,
        CancellationToken cancellationToken = default);
}
