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

    // ✅ NEW: membership
    public MembershipDetails Membership { get; private set; } = new MembershipDetails();

    // ✅ NEW: admin mapping
    public string? MappedAdminUserId { get; private set; }     // admin/manager userId
    public DateTime? AdminMappedAtUtc { get; private set; }
    public string? AdminMappedByUserId { get; private set; }   // who mapped it (superadmin etc.)


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

    // ✅ NEW: mapping admin/manager
    public void MapToAdmin(string adminUserId, string mappedByUserId)
    {
        MappedAdminUserId = adminUserId;
        AdminMappedByUserId = mappedByUserId;
        AdminMappedAtUtc = DateTime.UtcNow;
    }

    // ✅ NEW: membership operations
    public void ActivateMembership(DateTime startDateUtc, DateTime renewalDateUtc, decimal fee, PaymentDetails payment)
        => Membership.Activate(startDateUtc, renewalDateUtc, fee, payment);

    public void RenewMembership(DateTime renewalDateUtc, decimal fee, PaymentDetails payment)
        => Membership.Renew(renewalDateUtc, fee, payment);

    public void MarkMembershipExpired()
        => Membership.MarkExpired();
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

public class MembershipDetails
{
    public MembershipStatus Status { get; private set; } = MembershipStatus.None;

    public DateTime? MembershipStartDateUtc { get; private set; }
    public DateTime? RenewalDateUtc { get; private set; }

    public decimal? MembershipFee { get; private set; }
    public string Currency { get; private set; } = "INR";

    public PaymentDetails? LastPayment { get; private set; }

    // ✅ NEW: Payment history
    public List<PaymentDetails> PaymentHistory { get; private set; } = new();


    public MembershipDetails() { }

    public void Activate(DateTime startDateUtc, DateTime renewalDateUtc, decimal fee, PaymentDetails payment)
    {
        Status = MembershipStatus.Active;
        MembershipStartDateUtc = startDateUtc;
        RenewalDateUtc = renewalDateUtc;
        MembershipFee = fee;
        LastPayment = payment;
    }

    public void MarkExpired()
    {
        Status = MembershipStatus.Expired;
    }

    public void Renew(DateTime renewalDateUtc, decimal fee, PaymentDetails payment)
    {
        Status = MembershipStatus.Active;
        RenewalDateUtc = renewalDateUtc;
        MembershipFee = fee;
        LastPayment = payment;
    }

    public void Suspend(string reason, string? changedByAdminUserId = null)
    {
        Status = MembershipStatus.Suspended;
        // If you want, add SuspensionReason fields here.
    }
}

public class PaymentDetails
{
    public string PaymentId { get; private set; } = Guid.NewGuid().ToString();

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "INR";

    public PaymentMode Mode { get; private set; }
    public PaymentStatus Status { get; private set; }

    public DateTime PaidAtUtc { get; private set; } = DateTime.UtcNow;

    // external ref for Razorpay/Stripe/etc
    public string? GatewayTransactionId { get; private set; }

    private PaymentDetails() { }

    public PaymentDetails(decimal amount, PaymentMode mode, PaymentStatus status, string currency = "INR", string? gatewayTransactionId = null)
    {
        Amount = amount;
        Mode = mode;
        Status = status;
        Currency = currency;
        GatewayTransactionId = gatewayTransactionId;
    }
}