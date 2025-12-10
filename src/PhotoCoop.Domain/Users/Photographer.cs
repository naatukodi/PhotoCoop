namespace PhotoCoop.Domain.Users;

public class PhotographerProfile
{
    public string DisplayName { get; private set; }
    public string? Bio { get; private set; }
    public string? PortfolioUrl { get; private set; }

    public int YearsOfExperience { get; private set; }
    public decimal? AverageRating { get; private set; }
    public int TotalReviews { get; private set; }

    public List<RateCard> RateCards { get; private set; } = new();
    public List<OccasionType> Occasions { get; private set; } = new();

    private PhotographerProfile() { }

    public PhotographerProfile(string displayName, int yearsOfExperience)
    {
        DisplayName = displayName;
        YearsOfExperience = yearsOfExperience;
    }

    public void AddOccasion(OccasionType occasion)
    {
        if (!Occasions.Contains(occasion))
            Occasions.Add(occasion);
    }

    public void AddRateCard(RateCard card)
    {
        RateCards.Add(card);
    }
}

public class RateCard
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public OccasionType Occasion { get; private set; }
    public string PackageName { get; private set; }
    public decimal BasePrice { get; private set; }
    public decimal? PricePerHour { get; private set; }
    public decimal? PricePerDay { get; private set; }
    public string Currency { get; private set; } = "INR";

    private RateCard() { }

    public RateCard(OccasionType occasion, string packageName, decimal basePrice,
                    decimal? pricePerHour = null, decimal? pricePerDay = null)
    {
        Occasion = occasion;
        PackageName = packageName;
        BasePrice = basePrice;
        PricePerHour = pricePerHour;
        PricePerDay = pricePerDay;
    }
}
