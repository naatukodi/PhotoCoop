namespace PhotoCoop.Domain.Fundraising;

public enum FundraisingEventStatus
{
    Draft = 1,
    Active = 2,
    Paused = 3,
    Completed = 4,
    Cancelled = 5
}

public class FundraisingEvent : PhotoCoop.Domain.Common.Entity
{
    // Cosmos partition suggestion: /id or /organizerUserId (see notes)
    public string OrganizerUserId { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }

    public FundraisingEventStatus Status { get; private set; } = FundraisingEventStatus.Draft;

    public decimal? TargetAmount { get; private set; }
    public string Currency { get; private set; } = "INR";

    public DateTime StartDateUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? EndDateUtc { get; private set; }

    // ✅ Denormalized totals (recommended)
    public long TotalRaisedMinor { get; private set; } = 0; // paise
    public int SuccessfulDonationsCount { get; private set; } = 0;

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    private FundraisingEvent() { }

    public FundraisingEvent(string organizerUserId, string title, string? description, decimal? targetAmount, string currency, DateTime startDateUtc, DateTime? endDateUtc)
    {
        OrganizerUserId = organizerUserId;
        Title = title;
        Description = description;
        TargetAmount = targetAmount;
        Currency = currency;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        Status = FundraisingEventStatus.Active;
    }

    public void AddSuccessfulDonation(long amountMinor)
    {
        TotalRaisedMinor += amountMinor;
        SuccessfulDonationsCount += 1;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetStatus(FundraisingEventStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
