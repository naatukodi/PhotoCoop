using PhotoCoop.Domain.Common;
using PhotoCoop.Domain.Users;

namespace PhotoCoop.Domain.Bookings;

public class Booking : Entity
{
    public string CustomerUserId { get; private set; }
    public OccasionType Occasion { get; private set; }

    public DateTime EventDate { get; private set; }
    public TimeSpan? StartTime { get; private set; }
    public TimeSpan? EndTime { get; private set; }

    public Address EventAddress { get; private set; }

    public int? ExpectedHours { get; private set; }
    public int? ExpectedPhotographersCount { get; private set; }
    public decimal? BudgetMin { get; private set; }
    public decimal? BudgetMax { get; private set; }
    public string? SpecialRequirements { get; private set; }

    public BookingStatus Status { get; private set; } = BookingStatus.New;

    public string? AssignedPhotographerUserId { get; private set; }
    public string? AssignedByAdminUserId { get; private set; }

    public decimal? FinalAgreedPrice { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    public List<BookingStatusHistory> StatusHistory { get; private set; } = new();

    private Booking() { }

    public Booking(string customerUserId,
                   OccasionType occasion,
                   DateTime eventDate,
                   Address eventAddress,
                   int? expectedHours = null,
                   int? expectedPhotographersCount = null,
                   decimal? budgetMin = null,
                   decimal? budgetMax = null,
                   string? specialRequirements = null)
    {
        CustomerUserId = customerUserId;
        Occasion = occasion;
        EventDate = eventDate;
        EventAddress = eventAddress;
        ExpectedHours = expectedHours;
        ExpectedPhotographersCount = expectedPhotographersCount;
        BudgetMin = budgetMin;
        BudgetMax = budgetMax;
        SpecialRequirements = specialRequirements;

        AddStatusHistory(BookingStatus.New, null, "Booking created");
    }

    public void AssignPhotographer(string photographerUserId, string adminUserId, decimal? finalPrice = null)
    {
        AssignedPhotographerUserId = photographerUserId;
        AssignedByAdminUserId = adminUserId;
        FinalAgreedPrice = finalPrice;
        SetStatus(BookingStatus.Assigned, adminUserId, "Photographer assigned");
    }

    public void SetStatus(BookingStatus status, string? changedByUserId, string? remarks = null)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
        AddStatusHistory(status, changedByUserId, remarks);
    }

    private void AddStatusHistory(BookingStatus status, string? changedByUserId, string? remarks)
    {
        StatusHistory.Add(new BookingStatusHistory(status, changedByUserId, remarks));
    }
}

public class BookingStatusHistory
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public BookingStatus Status { get; private set; }
    public DateTime ChangedAtUtc { get; private set; } = DateTime.UtcNow;
    public string? ChangedByUserId { get; private set; }
    public string? Remarks { get; private set; }

    private BookingStatusHistory() { }

    public BookingStatusHistory(BookingStatus status, string? changedByUserId, string? remarks)
    {
        Status = status;
        ChangedByUserId = changedByUserId;
        Remarks = remarks;
    }
}
