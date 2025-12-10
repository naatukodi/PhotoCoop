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
