using PhotoCoop.Domain.Bookings;
using PhotoCoop.Domain.Common;
using PhotoCoop.Domain.Users;

namespace PhotoCoop.Application.Bookings;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<Booking> AssignPhotographerAsync(AssignPhotographerRequest request, CancellationToken cancellationToken = default);
    Task<Booking?> GetBookingAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetBookingsByCustomerAsync(string customerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetBookingsByPhotographerAsync(string photographerUserId, CancellationToken cancellationToken = default);
    Task<Booking> UpdateStatusAsync(UpdateBookingStatusRequest request, CancellationToken cancellationToken = default);
}

public class UpdateBookingStatusRequest
{
    public string BookingId { get; set; } = null!;
    public BookingStatus Status { get; set; }
    public string ChangedByUserId { get; set; } = null!;
    public string? Remarks { get; set; }
}

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;

    public BookingService(IBookingRepository bookingRepository, IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
    }

    public async Task<Booking> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _userRepository.GetByIdAsync(request.CustomerUserId, cancellationToken);
        if (customer == null || customer.UserType != UserType.Customer)
            throw new InvalidOperationException("Invalid customer.");

        var address = new Address(
            request.EventLine1,
            request.EventCity,
            request.EventState,
            request.EventPincode);

        var booking = new Booking(
            request.CustomerUserId,
            request.Occasion,
            request.EventDate,
            address,
            request.ExpectedHours,
            request.ExpectedPhotographersCount,
            request.BudgetMin,
            request.BudgetMax,
            request.SpecialRequirements);

        return await _bookingRepository.AddAsync(booking, cancellationToken);
    }

    public async Task<Booking> AssignPhotographerAsync(AssignPhotographerRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException("Booking not found.");

        var photographer = await _userRepository.GetByIdAsync(request.PhotographerUserId, cancellationToken);
        if (photographer == null || photographer.UserType != UserType.Photographer)
            throw new InvalidOperationException("Invalid photographer.");

        booking.AssignPhotographer(request.PhotographerUserId, request.AdminUserId, request.FinalPrice);

        return await _bookingRepository.UpdateAsync(booking, cancellationToken);
    }

    public Task<Booking?> GetBookingAsync(string id, CancellationToken cancellationToken = default)
        => _bookingRepository.GetByIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetBookingsByCustomerAsync(string customerUserId, CancellationToken cancellationToken = default)
        => await _bookingRepository.GetByCustomerAsync(customerUserId, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetBookingsByPhotographerAsync(string photographerUserId, CancellationToken cancellationToken = default)
        => await _bookingRepository.GetByPhotographerAsync(photographerUserId, cancellationToken);

    public async Task<Booking> UpdateStatusAsync(UpdateBookingStatusRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException("Booking not found.");

        booking.SetStatus(request.Status, request.ChangedByUserId, request.Remarks);
        return await _bookingRepository.UpdateAsync(booking, cancellationToken);
    }
}
