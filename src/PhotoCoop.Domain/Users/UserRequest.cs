using PhotoCoop.Domain.Users;

namespace PhotoCoop.Application.Users;

public class CreateCustomerUserRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public List<string> Pincodes { get; set; } = new();
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }

    public List<OccasionType> PreferredOccasions { get; set; } = new();
}

public class CreatePhotographerUserRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public List<string> Pincodes { get; set; } = new();

    public string DisplayName { get; set; } = null!;
    public string? Bio { get; set; }
    public string? PortfolioUrl { get; set; }
    public int YearsOfExperience { get; set; }

    public List<OccasionType> Occasions { get; set; } = new();

    public List<RateCardDto> RateCards { get; set; } = new();

    // ✅ NEW: Admin mapping (who manages this photographer)
    public string? MappedAdminUserId { get; set; }      // admin/manager userId
    public string? AdminMappedByUserId { get; set; }    // superadmin/admin who mapped

    // ✅ NEW: Membership initialization (optional at creation time)
    public MembershipInitDto? Membership { get; set; }
}

public class RateCardDto
{
    public OccasionType Occasion { get; set; }
    public string PackageName { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public decimal? PricePerHour { get; set; }
    public decimal? PricePerDay { get; set; }
}

public class CreateAdminUserRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public bool IsSuperAdmin { get; set; }
    public List<string> ManagedPincodes { get; set; } = new();
}
public class MembershipInitDto
{
    public DateTime MembershipStartDateUtc { get; set; }
    public DateTime RenewalDateUtc { get; set; }
    public decimal Fee { get; set; }
    public string Currency { get; set; } = "INR";

    public PaymentMode Mode { get; set; }
    public PaymentStatus Status { get; set; }
    public string? GatewayTransactionId { get; set; }
}